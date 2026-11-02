using System.Net;
using System.Text.Json;
using AutoMapper;
using NSubstitute;
using Shouldly;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Mappings;
using Void.Bot.Core.Services.ERA.Player;
using Void.Bot.Core.Services.ERA.Resonance;
using Void.Bot.Core.Services.Identity;
using Void.Bot.Core.Services.Language;
using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.UnitTest.ERA;

public sealed class EraResonanceDraftFixture
{


    [Fact]
    public async Task CombatRelations_SurviveOmittedSettingsAndOlderImports_AndEnterPublication()
    {
        var service=Service();
        var saved=await service.SaveDraftSettingsAsync(new(){ExpectedVersion=1,Settings=new(){CombatRelations=new(){Species=new(){new(){Id="construct",Name="构造体"}}}}},CancellationToken.None);
        saved.Code.ShouldBe(HttpStatusCode.OK);
        var legacy=await service.SaveDraftSettingsAsync(new(){ExpectedVersion=2,Settings=new()},CancellationToken.None);
        legacy.Code.ShouldBe(HttpStatusCode.OK);legacy.Data!.Settings.CombatRelations!.Species.Single().Id.ShouldBe("construct");
        var imported=await service.ImportDraftAsync(new(){ExpectedDraftRevision=_workspace.Revision,Mode=EraResonanceDraftImportMode.Replace,Configuration=new()},CancellationToken.None);
        imported.Code.ShouldBe(HttpStatusCode.OK);
        var published=await service.PublishDraftAsync(new(){ExpectedDraftRevision=_workspace.Revision,ExpectedPublishedVersion=_published.Version,RequestId=Guid.NewGuid()},CancellationToken.None);
        published.Code.ShouldBe(HttpStatusCode.OK);
        JsonSerializer.Deserialize<ResonanceConfiguration>(_published.Payload)!.CombatRelations!.Species.Single().Id.ShouldBe("construct");
        var cleared=await service.SaveDraftSettingsAsync(new(){ExpectedVersion=_workspace.SettingsVersion,Settings=new(){CombatRelations=new()}},CancellationToken.None);
        cleared.Code.ShouldBe(HttpStatusCode.OK);cleared.Data!.Settings.CombatRelations!.Species.ShouldBeEmpty();
    }




    private readonly IEraResonanceDataProvider _data=Substitute.For<IEraResonanceDataProvider>();
    private readonly ICurrentUser _user=Substitute.For<ICurrentUser>();
    private readonly Guid _account=Guid.NewGuid();
    private readonly EraResonanceDraftWorkspace _workspace=new(){Id=EraResonanceDataProvider.DraftWorkspaceId,Initialized=true,Revision=1,
        BasePublishedVersion=1,SettingsVersion=1,SettingsPayload=JsonSerializer.Serialize(new EraResonanceDraftSettings())};
    private readonly EraResonanceConfiguration _published=new(){Version=1,Payload=JsonSerializer.Serialize(new ResonanceConfiguration{Version=1})};
    private readonly List<EraResonanceDraftRecord> _records=new();
    private readonly Dictionary<long,EraResonancePublication> _history=new();
    private readonly Dictionary<Guid,EraResonancePublishReceipt> _receipts=new();
    private EraResonanceService Service()
    {
        _user.Id.Returns(_account);
        _data.GetDraftWorkspaceAsync(Arg.Any<bool>(),Arg.Any<CancellationToken>()).Returns(_workspace);
        _data.GetDraftRecordsAsync(Arg.Any<bool>(),Arg.Any<CancellationToken>()).Returns(_records);
        _data.GetConfigurationAsync(Arg.Any<bool>(),Arg.Any<CancellationToken>()).Returns(_published);
        _data.GetPublicationAsync(Arg.Any<long>(),Arg.Any<CancellationToken>()).Returns(c=>Task.FromResult<EraResonancePublication?>(_history.GetValueOrDefault(c.ArgAt<long>(0))));
        _data.GetPublishReceiptAsync(Arg.Any<Guid>(),Arg.Any<CancellationToken>()).Returns(c=>Task.FromResult<EraResonancePublishReceipt?>(_receipts.GetValueOrDefault(c.ArgAt<Guid>(0))));
        _data.AddPublicationAsync(Arg.Any<EraResonancePublication>(),Arg.Any<CancellationToken>()).Returns(c=>{var p=c.ArgAt<EraResonancePublication>(0);_history.Add(p.Version,p);return Task.CompletedTask;});
        _data.AddPublishReceiptAsync(Arg.Any<EraResonancePublishReceipt>(),Arg.Any<CancellationToken>()).Returns(c=>{var r=c.ArgAt<EraResonancePublishReceipt>(0);_receipts.Add(r.Id,r);return Task.CompletedTask;});
        var mapper=new Mapper(new MapperConfiguration(c=>c.AddProfile<EraResonanceMapping>()));
        return new(_data,Substitute.For<IEraCharacterContext>(),_user,Substitute.For<ILocalizationService>(),mapper);
    }
    private static ResonanceDefinition Resonance()=>new(){Id="test.skill",Name="原谐振技能",Variants=new(){new(){Slot=ResonanceSlotKind.Tactical,MaximumCharges=3,CooldownSeconds=2,CarrierKind=ResonanceCarrierKind.Self,Condition=new()}}};
    private EraResonanceDraftRecord AddResonance(bool deleted=false)
    {
        var record=new EraResonanceDraftRecord{Id=Guid.NewGuid(),WorkspaceId=_workspace.Id,Kind=EraResonanceDraftKind.Resonance,StableId="test.skill",Version=1,
            IsDeleted=deleted,Payload=JsonSerializer.Serialize(Resonance())};_records.Add(record);return record;
    }
    [Fact]
    public async Task SaveItem_DoesNotChangePublishedConfiguration()
    {
        var service=Service();var record=AddResonance();string original=_published.Payload;var skill=Resonance();skill.Name="修改后";
        var result=await service.SaveDraftResonanceAsync(new(){Id=skill.Id,ExpectedVersion=1,Definition=skill},CancellationToken.None);
        result.Code.ShouldBe(HttpStatusCode.OK);result.Data!.Version.ShouldBe(2);result.Data.DraftRevision.ShouldBe(2);
        _published.Payload.ShouldBe(original);_published.Version.ShouldBe(1);_history.ShouldBeEmpty();
    }
    [Fact]
    public async Task Publish_UnfinishedReferenceDoesNotMutatePublishedState()
    {
        var service=Service();_workspace.SettingsPayload=JsonSerializer.Serialize(new EraResonanceDraftSettings{CommonTacticalSkills=new(){"missing"}});
        string original=_published.Payload;
        var result=await service.PublishDraftAsync(new(){RequestId=Guid.NewGuid(),ExpectedDraftRevision=1,ExpectedPublishedVersion=1},CancellationToken.None);
        result.ErrorCode.ShouldBe("era.draft_invalid");_published.Payload.ShouldBe(original);_published.Version.ShouldBe(1);_history.ShouldBeEmpty();_receipts.ShouldBeEmpty();
    }
    [Fact]
    public async Task Publish_SameRequestRetriesSameSnapshotButNewRequestCreatesVersion()
    {
        var service=Service();var request=new PublishEraResonanceDraftCommand{RequestId=Guid.NewGuid(),ExpectedDraftRevision=1,ExpectedPublishedVersion=1};
        var first=await service.PublishDraftAsync(request,CancellationToken.None);
        var retry=await service.PublishDraftAsync(request,CancellationToken.None);
        first.Data!.Version.ShouldBe(2);retry.Data!.Version.ShouldBe(2);_history.Count.ShouldBe(1);
        var next=await service.PublishDraftAsync(new(){RequestId=Guid.NewGuid(),ExpectedDraftRevision=1,ExpectedPublishedVersion=2},CancellationToken.None);
        next.Data!.Version.ShouldBe(3);_history.Count.ShouldBe(2);
    }
    [Fact]
    public async Task Publish_SameRequestWithDifferentVersionsIsRejected()
    {
        var service=Service();var id=Guid.NewGuid();
        await service.PublishDraftAsync(new(){RequestId=id,ExpectedDraftRevision=1,ExpectedPublishedVersion=1},CancellationToken.None);
        var result=await service.PublishDraftAsync(new(){RequestId=id,ExpectedDraftRevision=1,ExpectedPublishedVersion=2},CancellationToken.None);
        result.ErrorCode.ShouldBe("era.publish_request_conflict");_published.Version.ShouldBe(2);
    }
    [Fact]
    public async Task Delete_DoesNotTouchPlayersAndCannotSilentlyRecreateId()
    {
        var service=Service();var record=AddResonance();
        var deleted=await service.DeleteDraftResonanceAsync(new(){Id="test.skill",ExpectedVersion=1},CancellationToken.None);
        deleted.Code.ShouldBe(HttpStatusCode.OK);record.IsDeleted.ShouldBeTrue();
        var result=await service.SaveDraftResonanceAsync(new(){Id="test.skill",ExpectedVersion=2,Definition=Resonance()},CancellationToken.None);
        result.ErrorCode.ShouldBe("era.draft_item_deleted");
        _data.DidNotReceive().UpdateProfile(Arg.Any<EraCharacterResonanceProfile>());
        await _data.DidNotReceive().GrantResonanceAsync(Arg.Any<EraCharacterLearnedResonance>(),Arg.Any<CancellationToken>());
    }
    [Fact]
    public async Task ImportReplace_MarksOmittedObjectsDeletedWithoutPublishing()
    {
        var service=Service();var record=AddResonance();var original=_published.Payload;
        var result=await service.ImportDraftAsync(new(){ExpectedDraftRevision=1,Mode=EraResonanceDraftImportMode.Replace,Configuration=new()},CancellationToken.None);
        result.Code.ShouldBe(HttpStatusCode.OK);record.IsDeleted.ShouldBeTrue();_published.Payload.ShouldBe(original);_history.ShouldBeEmpty();
    }
    [Fact]
    public async Task Get_UninitializedDraftDoesNotCreateData()
    {
        var service=Service();_workspace.Initialized=false;
        var result=await service.GetDraftStatusAsync(new(),CancellationToken.None);
        result.ErrorCode.ShouldBe("era.draft_unavailable");_workspace.Revision.ShouldBe(1);
        await _data.DidNotReceive().AddDraftRecordAsync(Arg.Any<EraResonanceDraftRecord>(),Arg.Any<CancellationToken>());
    }
    [Fact]
    public async Task Publish_StaleDraftRevisionDoesNotPublish()
    {
        var service=Service();
        var result=await service.PublishDraftAsync(new(){RequestId=Guid.NewGuid(),ExpectedDraftRevision=0,ExpectedPublishedVersion=1},CancellationToken.None);
        result.ErrorCode.ShouldBe("era.draft_version_conflict");_published.Version.ShouldBe(1);_receipts.ShouldBeEmpty();
    }

    [Fact]
    public async Task RestoreHistory_OnlyChangesDraftThenPublishesNewMonotonicVersion()
    {
        var service=Service();var record=AddResonance();
        _history.Add(0,new(){Id=Guid.NewGuid(),Version=0,Payload=JsonSerializer.Serialize(new ResonanceConfiguration{Version=0})});
        var restored=await service.RestorePublicationToDraftAsync(new(){Version=0,ExpectedDraftRevision=1},CancellationToken.None);
        restored.Code.ShouldBe(HttpStatusCode.OK);record.IsDeleted.ShouldBeTrue();_published.Version.ShouldBe(1);
        var published=await service.PublishDraftAsync(new(){RequestId=Guid.NewGuid(),ExpectedDraftRevision=2,ExpectedPublishedVersion=1},CancellationToken.None);
        published.Data!.Version.ShouldBe(2);_history.ContainsKey(0).ShouldBeTrue();_history.ContainsKey(2).ShouldBeTrue();
    }
    [Fact]
    public async Task ReadChangedDraft_PreservesAvailablePublishingVersion()
    {
        var service=Service();AddResonance();
        var status=await service.GetDraftStatusAsync(new(),CancellationToken.None);
        status.Data!.HasUnpublishedChanges.ShouldBeTrue();status.Data.PublishedVersion.ShouldBe(1);
        _published.Version.ShouldBe(1);_history.ShouldBeEmpty();
    }
}
