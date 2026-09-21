const { test } = require('node:test');
const assert = require('node:assert/strict');
const { validate, upsert, ancestors, layout } = require('./family-tree-model.js');
const root = { id: 'a', name: 'Oyster master', species: 'Oyster', date: '2026-01-01', method: 'Source', parentIds: [], notes: '' };
const child = { ...root, id: 'b', name: 'Clone', method: 'Clone', parentIds: ['a'] };
test('visual roots put children below all parents and retain both cross connections', () => {
  assert.equal(typeof layout, 'function', 'Visual root layout is missing');
  const graph = layout([root, child, { ...root, id: 'c' }, { ...child, id: 'd', method: 'Cross', parentIds: ['b', 'c'] }]);
  const byId = new Map(graph.nodes.map(node => [node.id, node]));
  assert.ok(byId.get('d').y > byId.get('b').y && byId.get('d').y > byId.get('c').y);
  assert.deepEqual(graph.edges.filter(edge => edge.child === 'd').map(edge => edge.parent).sort(), ['b', 'c']);
  assert.ok(Math.abs(byId.get('a').x - byId.get('c').x) >= 200, 'Source name cards overlap');
});
test('edits retain stable links and ancestry includes both parents of a cross', () => {
  let records = upsert([], root); records = upsert(records, child);
  records = upsert(records, { ...root, id: 'c', name: 'Second parent' });
  records = upsert(records, { ...child, id: 'd', method: 'Cross', parentIds: ['b', 'c'] });
  records = upsert(records, { ...root, name: 'Renamed master' });
  assert.equal(records.length, 4); assert.deepEqual(new Set(ancestors(records, 'd')), new Set(['a', 'b', 'c']));
});
test('invalid links, cycles, relationship counts and dates are rejected without mutation', () => {
  const records = [root, child];
  for (const edit of [
    { ...root, method: 'Transfer', parentIds: ['b'] }, { ...child, parentIds: ['missing'] },
    { ...child, parentIds: ['b'] }, { ...child, method: 'Cross', parentIds: ['a', 'a'] },
    { ...child, date: '2025-01-01' }, { ...child, date: '2026-02-30' }, { ...child, name: ' ' },
    { ...child, method: 'Cross' }
  ]) assert.throws(() => upsert(records, edit));
  assert.equal(records[0].name, 'Oyster master'); assert.equal(records.length, 2);
  assert.throws(() => validate([root, root]));
});
