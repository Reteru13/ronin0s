(() => {
  'use strict';
  const $ = id => document.getElementById(id);
  const key = 'ronin.genetics-observations.v1';
  const conditions = ['Not rated', 'Strong', 'Steady', 'Weak', 'Needs attention'];
  let records = [], editing = null, blocked = false;
  const status = (text, error = false) => { $('genetics-status').textContent = text; $('genetics-status').classList.toggle('error', error); };
  function validate(rows) {
    if (!Array.isArray(rows) || rows.length > 10000) throw Error('Up to 10,000 observations are supported.');
    const ids = new Set();
    for (const row of rows) {
      if (!row || typeof row.id !== 'string' || !row.id || ids.has(row.id)) throw Error('Invalid or duplicate observation ID.'); ids.add(row.id);
      if (typeof row.culture !== 'string' || !row.culture.trim() || row.culture.length > 120) throw Error('Enter a culture name (up to 120 characters).');
      if (typeof row.notes !== 'string' || !row.notes.trim() || row.notes.length > 20000) throw Error('Enter observation notes (up to 20,000 characters).');
      if (typeof row.date !== 'string' || !/^\d{4}-\d{2}-\d{2}$/.test(row.date) || row.date < '0001-01-01' || !Number.isFinite(Date.parse(row.date)) || new Date(row.date).toISOString().slice(0, 10) !== row.date || row.date > CultureTree.today()) throw Error('Choose a valid observation date, today or earlier.');
      if (row.passage !== null && (!Number.isInteger(row.passage) || row.passage < 0 || row.passage > 9999)) throw Error('Transfer number must be a whole number from 0 to 9999, or blank.');
      if (!conditions.includes(row.condition)) throw Error('Choose a condition.');
    }
    return rows;
  }
  function node(tag, text) { const e = document.createElement(tag); e.textContent = text; return e; }
  function edit(row = null) {
    editing = row?.id || null;
    $('genetics-editor-title').textContent = row ? 'Edit observation' : 'New observation';
    $('genetics-culture').value = row?.culture || '';
    $('genetics-date').value = row?.date || CultureTree.today();
    $('genetics-date').max = CultureTree.today();
    $('genetics-passage').value = row?.passage ?? '';
    $('genetics-condition').value = row?.condition || 'Not rated';
    $('genetics-notes').value = row?.notes || '';
    $('genetics-cancel').hidden = !row;
    $('genetics-save').textContent = row ? 'Save changes' : 'Save observation';
  }
  function render() {
    const query = $('genetics-search').value.trim().toLowerCase();
    const rows = records.filter(row => `${row.culture} ${row.condition} ${row.notes}`.toLowerCase().includes(query)).sort((a, b) => b.date.localeCompare(a.date));
    $('genetics-count').textContent = `${rows.length} saved observations`;
    const history = $('genetics-history'); history.replaceChildren();
    if (!rows.length) history.append(node('p', query ? 'No matching observations.' : 'No observations yet. Save your first entry using the form.'));
    for (const row of rows.slice(0, 100)) {
      const card = node('article', ''); card.className = 'observation-card';
      const notes = node('p', row.notes); notes.className = 'observation-notes';
      card.append(node('h4', row.culture), node('p', `${row.date} · ${row.condition} · ${row.passage === null ? 'Transfer not recorded' : 'Transfer ' + row.passage}`), notes);
      const button = node('button', 'Edit observation'); button.type = 'button'; button.setAttribute('aria-label', `Edit observation for ${row.culture} on ${row.date}`);
      button.onclick = () => { edit(row); $('genetics-culture').focus(); }; card.append(button); history.append(card);
    }
    if (rows.length > 100) history.append(node('p', 'Showing the newest 100 matches. Narrow your search for older observations.'));
  }
  function suggestions() {
    const list = $('genetics-cultures'); list.replaceChildren();
    try {
      const cultures = CultureTree.validate(JSON.parse(localStorage.getItem('ronin.culture-family-tree.v1') || '[]'));
      for (const name of new Set([...cultures.map(row => row.name), ...records.map(row => row.culture)])) list.append(new Option(name, name));
    } catch { /* Observation entry remains available if the separate tree cannot be read. */ }
  }
  $('genetics-form').addEventListener('submit', event => {
    event.preventDefault(); if (blocked) return status('Reload after resolving browser storage before saving. Existing records are preserved.', true);
    try {
      const rawPassage = $('genetics-passage').value.trim();
      const row = { id: editing || crypto.randomUUID(), culture: $('genetics-culture').value.trim(), date: $('genetics-date').value, passage: rawPassage === '' ? null : Number(rawPassage), condition: $('genetics-condition').value, notes: $('genetics-notes').value.trim() };
      const next = validate([...records.filter(record => record.id !== row.id), row]);
      localStorage.setItem(key, JSON.stringify(next)); records = next;
      $('genetics-search').value = ''; render(); edit(); suggestions(); status('Observation saved in this browser. Use Edit observation to make changes.');
    } catch (error) { status('Not saved: ' + error.message, true); }
  });
  $('genetics-search').addEventListener('input', render);
  $('genetics-culture').addEventListener('focus', suggestions);
  $('genetics-cancel').onclick = () => { edit(); status('Editing cancelled. Your saved observation is unchanged.'); };
  $('genetics-export').onclick = () => {
    const url = URL.createObjectURL(new Blob([JSON.stringify(records, null, 2)], { type: 'application/json' }));
    const link = node('a', ''); link.href = url; link.download = 'ronin-genetics-observations.json'; link.click(); setTimeout(() => URL.revokeObjectURL(url), 1000);
  };
  window.addEventListener('storage', event => { if (event.key === key || event.key === null) { blocked = true; status('Observations changed in another tab. Reload before saving.', true); } });
  try { records = validate(JSON.parse(localStorage.getItem(key) || '[]')); }
  catch (error) { blocked = true; status('Saved observations could not be loaded: ' + error.message + ' Existing storage is preserved.', true); }
  edit(); render(); suggestions();
})();
