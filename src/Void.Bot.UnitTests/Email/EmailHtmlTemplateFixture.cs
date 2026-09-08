using Shouldly;
using Void.Bot.Core.Services.Email;

namespace Void.Bot.UnitTest.Email;

public sealed class EmailHtmlTemplateFixture
{
    private static readonly EmailTemplateText TemplateText = new(
        "en",
        "Void Bot Team",
        "This is an automated message from Void Bot. Please do not reply.");

    [Fact]
    public void RenderCreatesStandardCardLayout()
    {
        var html = EmailHtmlTemplate.Render(
            "Verification code",
            "Your code is:\n123456",
            TemplateText);

        html.ShouldContain("VOID BOT");
        html.ShouldContain(">Verification code</h1>");
        html.ShouldContain("Your code is:\n123456");
        html.ShouldContain("white-space:pre-line");
        html.ShouldContain("Void Bot Team");
        html.ShouldContain("This is an automated message from Void Bot. Please do not reply.");
    }

    [Fact]
    public void RenderEncodesSubjectAndContent()
    {
        var html = EmailHtmlTemplate.Render(
            "Reset <password>",
            "Click <a href=\"bad\">here</a> & continue",
            TemplateText);

        html.ShouldContain("Reset &lt;password&gt;");
        html.ShouldContain("Click &lt;a href=&quot;bad&quot;&gt;here&lt;/a&gt; &amp; continue");
        html.ShouldNotContain("<a href=\"bad\">");
    }

    [Fact]
    public void RenderVerificationCodeCreatesCenteredCodeBlock()
    {
        var html = EmailHtmlTemplate.RenderVerificationCode(
            "Verification code",
            "Your verification code is:",
            "123456",
            "This code expires in 10 minutes.",
            TemplateText);

        html.ShouldContain("align=\"center\"");
        html.ShouldContain("border-radius:10px");
        html.ShouldContain("background-color:#f3f4f6");
        html.ShouldContain(">123456</span>");
        html.ShouldContain("This code expires in 10 minutes.");
    }

    [Fact]
    public void RenderUsesLocalizedTemplateText()
    {
        var html = EmailHtmlTemplate.Render(
            "邮箱验证码",
            "你的验证码是 123456。",
            new EmailTemplateText("zh-chs", "Void Bot 团队", "此邮件由系统自动发送。"));

        html.ShouldContain("lang=\"zh-chs\"");
        html.ShouldContain("Void Bot 团队");
        html.ShouldContain("此邮件由系统自动发送。");
    }
}
