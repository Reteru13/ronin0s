window.drawMycelium = function (host, records, selectedId, onSelect, onToggle, collapsed, branches, onAdd) {
  const graph = CultureTree.layout(records);
  const positions = new Map(graph.nodes.map(point => [point.id, point]));
  const highlight = new Set([...CultureTree.ancestors(records, selectedId), selectedId]);
  const canvas = document.createElement('div'); canvas.className = 'mycelium-canvas'; canvas.style.width = graph.width + 'px'; canvas.style.height = graph.height + 'px';
  const ns = 'http://www.w3.org/2000/svg';
  const svg = document.createElementNS(ns, 'svg'); svg.setAttribute('width', graph.width); svg.setAttribute('height', graph.height); svg.setAttribute('aria-hidden', 'true');
  function path(d, stroke, width = 2, fill = 'none', className = '') {
    const p = document.createElementNS(ns, 'path'); p.setAttribute('d', d); p.setAttribute('stroke', stroke); p.setAttribute('stroke-width', width); p.setAttribute('fill', fill); p.setAttribute('stroke-linecap', 'round'); if (className) p.setAttribute('class', className); svg.append(p); return p;
  }
  const cx = graph.width / 2;
  // The mushroom is a collection symbol. Only labelled nodes represent cultures.
  path(`M ${cx-21} 114 C ${cx-15} 151 ${cx-31} 185 ${cx-24} 218 Q ${cx} 233 ${cx+25} 218 C ${cx+16} 182 ${cx+27} 145 ${cx+20} 114 Z`, '#b6d6c3', 2, '#dee9d9');
  path(`M ${cx-145} 112 C ${cx-113} 31 ${cx-53} 16 ${cx} 28 C ${cx+62} 9 ${cx+122} 55 ${cx+145} 112 Q ${cx} 166 ${cx-145} 112 Z`, '#c9e8d0', 2, '#78a893');
  path(`M ${cx-145} 112 Q ${cx} 95 ${cx+145} 112`, '#cce4cc', 2);
  for (let i = -5; i <= 5; i++) path(`M ${cx+i*24} 119 Q ${cx+i*12} 138 ${cx+i*3} 143`, '#e8efcd', 1);
  path(`M ${cx-103} 84 Q ${cx-67} 42 ${cx-23} 46`, '#d8edcd', 3);
  path(`M 35 231 Q ${cx} 221 ${graph.width-35} 231`, '#294738', 1);
  for (const row of records) {
    const p = positions.get(row.id);
    if (!row.parentIds.length) path(`M ${cx} 219 C ${cx} 243 ${p.x} 227 ${p.x} ${p.y}`, '#749e7e', 3);
  }
  for (const edge of graph.edges) {
    const a = positions.get(edge.parent), b = positions.get(edge.child), start = a.y + 66;
    let d = `M ${a.x} ${start} C ${a.x} ${start+52} ${b.x} ${b.y-52} ${b.x} ${b.y}`;
    if (b.depth - a.depth > 1) {
      const side = a.x < cx ? 24 : graph.width - 24;
      d = `M ${a.x} ${start} C ${a.x} ${start+35} ${side} ${start+35} ${side} ${start+65} L ${side} ${b.y-65} C ${side} ${b.y-25} ${b.x} ${b.y-40} ${b.x} ${b.y}`;
    }
    const active = highlight.has(edge.parent) && highlight.has(edge.child);
    const line = path(d, active ? '#b4efb6' : '#648a70', active ? 4 : 2.5, 'none', 'mycelium-link');
    line.dataset.parent = edge.parent; line.dataset.child = edge.child;
    path(`M ${b.x-4} ${b.y-9} L ${b.x} ${b.y-2} L ${b.x+4} ${b.y-9}`, active ? '#b4efb6' : '#648a70', 2);
  }
  for (const row of records) {
    const p = positions.get(row.id);
    if (!records.some(child => child.parentIds.includes(row.id))) {
      for (const spread of [-54, -20, 24, 58]) path(`M ${p.x} ${p.y+66} C ${p.x} ${p.y+98} ${p.x+spread} ${p.y+105} ${p.x+spread*1.25} ${p.y+133}`, '#355d45', 1.4);
    }
  }
  canvas.append(svg);
  for (const row of records) {
    const p = positions.get(row.id);
    const button = document.createElement('button'); button.type = 'button'; button.className = 'culture-node root-name'; button.classList.toggle('selected', row.id === selectedId); button.classList.toggle('ancestor', highlight.has(row.id));
    button.style.left = p.x - 90 + 'px'; button.style.top = p.y + 'px'; button.setAttribute('aria-label', 'Edit ' + row.name);
    const name = document.createElement('strong'); name.textContent = row.name;
    const method = document.createElement('span'); method.textContent = row.method + (row.parentIds.length === 2 ? ' · 2 parents' : '');
    button.append(name, method); button.title = `${row.name}\n${row.species}\n${row.date}`; button.onclick = () => onSelect(row); canvas.append(button);
    if (onToggle && branches.has(row.id)) {
      const toggle = document.createElement('button'); toggle.type = 'button'; toggle.className = 'root-toggle'; toggle.style.left = p.x + 72 + 'px'; toggle.style.top = p.y + 51 + 'px'; toggle.textContent = collapsed.has(row.id) ? '+' : '−'; toggle.setAttribute('aria-label', `${collapsed.has(row.id) ? 'Expand' : 'Collapse'} descendants of ${row.name}`); toggle.onclick = () => onToggle(row.id); canvas.append(toggle);
    }
    if (!branches.has(row.id)) {
      const add = document.createElement('button'); add.type = 'button'; add.className = 'root-add'; add.textContent = '+ Add child'; add.style.left = p.x - 52 + 'px'; add.style.top = p.y + 110 + 'px'; add.setAttribute('aria-label', 'Add child of ' + row.name); add.onclick = () => onAdd(row); canvas.append(add);
    }
  }
  host.append(canvas); return graph;
};
