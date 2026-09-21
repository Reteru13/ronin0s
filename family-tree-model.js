(function (scope) {
  'use strict';
  const methods = ['Source', 'Transfer', 'Clone', 'Isolate', 'Cross'];
  function today() {
    const now = new Date();
    return `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}-${String(now.getDate()).padStart(2, '0')}`;
  }
  function validate(records) {
    if (!Array.isArray(records) || records.length > 1000) throw Error('Family tree supports up to 1,000 cultures.');
    const map = new Map();
    for (const row of records) {
      if (!row || typeof row.id !== 'string' || !row.id || map.has(row.id)) throw Error('Invalid or duplicate culture ID.');
      map.set(row.id, row);
    }
    for (const row of records) {
      for (const field of ['name', 'species', 'notes']) {
        if (typeof row[field] !== 'string' || row[field].length > (field === 'notes' ? 20000 : 120) || (field !== 'notes' && !row[field].trim())) throw Error('Enter a name and species / strain; keep notes under 20,000 characters.');
      }
      if (typeof row.date !== 'string' || !/^\d{4}-\d{2}-\d{2}$/.test(row.date) || row.date < '0001-01-01' || !Number.isFinite(Date.parse(row.date)) || new Date(row.date).toISOString().slice(0, 10) !== row.date || row.date > today()) throw Error('Enter a valid culture date, today or earlier.');
      if (!methods.includes(row.method)) throw Error('Choose a relationship type.');
      const parents = row.parentIds;
      if (!Array.isArray(parents) || parents.length > 2 || new Set(parents).size !== parents.length || parents.some(id => id === row.id || !map.has(id))) throw Error('Choose different existing parents; a culture cannot be its own parent.');
      const required = row.method === 'Source' ? 0 : row.method === 'Cross' ? 2 : 1;
      if (parents.length !== required) throw Error('Source needs no parents; transfer, clone and isolate need one; cross needs two.');
      if (parents.some(id => map.get(id).date > row.date)) throw Error('A culture cannot predate its parent.');
    }
    const depths = new Map();
    for (let pass = 0; pass <= records.length; pass++) {
      const ready = records.filter(row => !depths.has(row.id) && row.parentIds.every(id => depths.has(id)));
      if (!ready.length) break;
      for (const row of ready) {
        const depth = row.parentIds.length ? Math.max(...row.parentIds.map(id => depths.get(id))) + 1 : 0;
        if (depth > 64) throw Error('Family tree supports up to 64 ancestry levels.');
        depths.set(row.id, depth);
      }
    }
    if (depths.size !== records.length) throw Error('Parent links would create circular ancestry.');
    return records;
  }
  function upsert(records, record) {
    return validate([...records.filter(row => row.id !== record.id), { ...record, parentIds: [...record.parentIds] }]);
  }
  function ancestors(records, id) {
    const map = new Map(records.map(row => [row.id, row]));
    const found = new Set(); const queue = [...(map.get(id)?.parentIds || [])];
    while (queue.length) { const parent = queue.shift(); if (found.has(parent)) continue; found.add(parent); queue.push(...(map.get(parent)?.parentIds || [])); }
    return [...found];
  }
  function layout(records) {
    const depths = new Map();
    for (let pass = 0; pass <= records.length; pass++) {
      const ready = records.filter(row => !depths.has(row.id) && row.parentIds.every(id => depths.has(id)));
      if (!ready.length) break;
      for (const row of ready) depths.set(row.id, row.parentIds.length ? Math.max(...row.parentIds.map(id => depths.get(id))) + 1 : 0);
    }
    const levels = new Map();
    for (const row of records) { const depth = depths.get(row.id); if (!levels.has(depth)) levels.set(depth, []); levels.get(depth).push(row); }
    const width = Math.max(680, (Math.max(0, ...[...levels.values()].map(rows => rows.length)) + 1) * 220 + 120);
    const nodes = [], positions = new Map();
    for (const [depth, rows] of [...levels].sort((a, b) => a[0] - b[0])) {
      const center = row => row.parentIds.length ? row.parentIds.reduce((sum, id) => sum + positions.get(id).x, 0) / row.parentIds.length : 0;
      rows.sort((a, b) => center(a) - center(b) || a.name.localeCompare(b.name) || a.id.localeCompare(b.id));
      rows.forEach((row, index) => {
        const point = { id: row.id, x: (width - 120) * (index + 1) / (rows.length + 1) + 60, y: 260 + depth * 165, depth };
        nodes.push(point); positions.set(row.id, point);
      });
    }
    return { width, height: Math.max(530, 430 + Math.max(0, ...depths.values()) * 165), nodes, edges: records.flatMap(row => row.parentIds.map(parent => ({ parent, child: row.id }))) };
  }
  const api = { validate, upsert, ancestors, today, methods, layout };
  if (typeof module !== 'undefined' && module.exports) module.exports = api;
  else scope.CultureTree = api;
})(globalThis);
