(() => {
  'use strict';
  const model = window.CultureTree;
  const key = 'ronin.culture-family-tree.v1';
  const $ = id => document.getElementById(id);
  const form = $('culture-form');
  const tree = $('culture-tree');
  let records = [], selectedId = null, storageBlocked = false;
  const collapsed = new Set();
  const message = (text, error = false) => { $('culture-status').textContent = text; $('culture-status').classList.toggle('error', error); };
  function element(tag, text, className) {
    const node = document.createElement(tag); if (text !== undefined) node.textContent = text; if (className) node.className = className; return node;
  }
  function edit(record = { id: crypto.randomUUID(), name: '', species: '', date: model.today(), method: 'Source', parentIds: [], notes: '' }) {
    selectedId = record.id;
    $('culture-editor-title').textContent = records.some(row => row.id === record.id) ? 'Culture details' : 'New culture';
    for (const field of ['name', 'species', 'date', 'method', 'notes']) $('culture-' + field).value = record[field];
    for (let index = 0; index < 2; index++) {
      const select = $('culture-parent-' + index); select.replaceChildren(new Option('No parent', ''));
      for (const row of [...records].sort((a, b) => a.name.localeCompare(b.name))) if (row.id !== record.id) select.add(new Option(row.name, row.id));
      select.value = record.parentIds[index] || '';
    }
    $('culture-add-child').hidden = !records.some(row => row.id === record.id);
    const ancestry = $('culture-ancestry'); ancestry.replaceChildren();
    const ids = model.ancestors(records, record.id);
    if (ids.length) {
      ancestry.append(element('h4', 'Ancestry — select to inspect'));
      for (const id of ids) {
        const parent = records.find(row => row.id === id); const button = element('button', parent.name, 'ancestor-link');
        button.type = 'button'; button.onclick = () => { edit(parent); render(); }; ancestry.append(button);
      }
    }
    render();
  }
  function render() {
    tree.replaceChildren();
    $('culture-count').textContent = `${records.length} cultures · ${records.filter(row => !row.parentIds.length).length} sources`;
    const query = $('culture-search').value.trim().toLowerCase();
    const matches = records.filter(row => `${row.name} ${row.species} ${row.notes}`.toLowerCase().includes(query));
    const visible = new Set(matches.map(row => row.id));
    for (const row of matches) for (const id of model.ancestors(records, row.id)) visible.add(id);
    const shown = records.filter(row => visible.has(row.id) && (query || !model.ancestors(records, row.id).some(id => collapsed.has(id))));
    const branches = new Set(records.flatMap(row => row.parentIds));
    const graph = window.drawMycelium(tree, shown, selectedId, edit, id => {
      if (collapsed.has(id)) collapsed.delete(id); else collapsed.add(id);
      render();
    }, collapsed, branches, parent => {
      edit({ id: crypto.randomUUID(), name: '', species: parent.species, date: model.today(), method: 'Transfer', parentIds: [parent.id], notes: '' });
      $('culture-name').focus();
    });
    if (!shown.length) {
      const note = element('p', records.length ? 'No matching cultures.' : 'Add your first source culture to start the roots.', 'tree-empty');
      note.style.position = 'absolute'; note.style.top = '270px'; note.style.left = (graph.width / 2 - 140) + 'px'; note.style.width = '280px';
      tree.querySelector('.mycelium-canvas').append(note);
    }
    applyZoom();
  }
  let zoom = .78;
  function applyZoom() {
    const canvas = tree.querySelector('.mycelium-canvas');
    if (canvas) canvas.style.zoom = zoom;
    $('culture-zoom-label').textContent = Math.round(zoom * 100) + '%';
  }
  $('culture-zoom-in').onclick = () => { zoom = Math.min(1.5, zoom + 0.1); applyZoom(); };
  $('culture-zoom-out').onclick = () => { zoom = Math.max(0.4, zoom - 0.1); applyZoom(); };
  $('culture-fit').onclick = () => {
    const canvas = tree.querySelector('.mycelium-canvas');
    if (canvas) { zoom = Math.max(0.4, Math.min(1, (tree.clientWidth - 20) / parseFloat(canvas.style.width))); applyZoom(); tree.scrollLeft = 0; tree.scrollTop = 0; }
  };
  form.addEventListener('submit', event => {
    event.preventDefault();
    if (storageBlocked) return message('Saved records could not be loaded. Reload after resolving browser storage; existing data has not been overwritten.', true);
    try {
      const record = { id: selectedId, parentIds: [0, 1].map(i => $('culture-parent-' + i).value).filter(Boolean) };
      for (const field of ['name', 'species', 'date', 'method', 'notes']) record[field] = $('culture-' + field).value.trim();
      const next = model.upsert(records, record);
      localStorage.setItem(key, JSON.stringify(next));
      records = next; $('culture-search').value = ''; edit(record); message('Culture saved in this browser.');
    } catch (error) { message('Not saved: ' + error.message, true); }
  });
  $('culture-new').onclick = () => { edit(); $('culture-name').focus(); };
  $('culture-add-child').onclick = () => {
    const parent = records.find(row => row.id === selectedId);
    edit({ id: crypto.randomUUID(), name: '', species: parent.species, date: model.today(), method: 'Transfer', parentIds: [parent.id], notes: '' });
    $('culture-name').focus();
  };
  $('culture-search').addEventListener('input', render);
  $('culture-expand').onclick = () => { collapsed.clear(); render(); };
  $('culture-collapse').onclick = () => { records.flatMap(row => row.parentIds).forEach(id => collapsed.add(id)); render(); };
  $('culture-export').onclick = () => {
    const url = URL.createObjectURL(new Blob([JSON.stringify(records, null, 2)], { type: 'application/json' }));
    const link = element('a'); link.href = url; link.download = 'ronin-family-tree.json'; link.click(); setTimeout(() => URL.revokeObjectURL(url), 1000);
  };
  try { const saved = localStorage.getItem(key); if (saved !== null) records = model.validate(JSON.parse(saved)); }
  catch (error) { storageBlocked = true; message('Saved tree could not be loaded: ' + error.message + ' Existing storage has been preserved.', true); }
  window.addEventListener('storage', event => {
    if (event.key === key || event.key === null) { storageBlocked = true; message('Records changed in another tab. Reload before saving to avoid overwriting changes.', true); }
  });
  edit(records[0]);
})();
