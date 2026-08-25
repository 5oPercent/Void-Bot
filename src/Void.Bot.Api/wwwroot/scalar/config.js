export default {
  tagsSorter: (left, right) => {
    const leftOrder = left['x-order'] ?? Number.MAX_SAFE_INTEGER
    const rightOrder = right['x-order'] ?? Number.MAX_SAFE_INTEGER

    return leftOrder - rightOrder || left.name.localeCompare(right.name)
  },
}
