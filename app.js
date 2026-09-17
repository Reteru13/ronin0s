const recipes = [
  { cat: 'agar', title: 'Light Malt Extract Agar', ingredients: '20g agar, 20g light malt, 1L water', thermal: '121°C @ 20 min', bestFor: 'Tissue transfers', pros: ['Clear view','Fast growth','Low cost'], cons: ['Sensitive to excess malt'] },
  { cat: 'agar', title: 'Potato Dextrose Yeast Agar', ingredients: 'Potato infusion, dextrose, yeast extract', thermal: '121°C @ 25 min', bestFor: 'Weak strains', pros: ['Nutrient dense','Strong rhizomorphs'], cons: ['Cloudy surface'] },
  { cat: 'agar', title: 'Water Agar', ingredients: '20g agar, 1L distilled water', thermal: '121°C @ 15 min', bestFor: 'Wild culture rescue', pros: ['Low contamination pressure','Simple prep'], cons: ['Slow culture growth'] },
  { cat: 'agar', title: 'Dog Kibble Yeast Agar', ingredients: 'Ground kibble, yeast, agar', thermal: '121°C @ 30 min', bestFor: 'Aggressive sector selection', pros: ['Cheap','High enzyme activity'], cons: ['Opaque dish'] },
  { cat: 'agar', title: 'Cornmeal Glucose Peptone Agar', ingredients: 'Cornmeal, glucose, peptone', thermal: '121°C @ 20 min', bestFor: 'Slants and storage', pros: ['Stable long-term culture','Good nitrogen balance'], cons: ['Requires peptone'] },
  { cat: 'grain', title: 'Whole Red Winter Wheat', ingredients: '100% wheat, gypsum, soak', thermal: '121°C @ 90 min', bestFor: 'G2G propagation', pros: ['Resistant to bursting','Uniform moisture'], cons: ['Requires soak'] },
  { cat: 'grain', title: 'Whole White Millet', ingredients: 'Millet + water direct', thermal: '121°C @ 120 min', bestFor: 'High inoculation points', pros: ['No soak needed','Fast colonization'], cons: ['Can clump'] },
  { cat: 'grain', title: 'Racehorse Oats', ingredients: 'Whole oats, 12h soak', thermal: '121°C @ 90 min', bestFor: 'Low-cost production', pros: ['Very affordable','Durable hulls'], cons: ['Extra debris'] },
  { cat: 'grain', title: 'Rye Berries', ingredients: 'Rye grain, gypsum soak', thermal: '121°C @ 90 min', bestFor: 'Lab-standard vigor', pros: ['High nutrient profile','Strong culture health'], cons: ['More expensive'] },
  { cat: 'grain', title: 'Sorghum / Milo', ingredients: 'Milo grain, soak + boil', thermal: '121°C @ 90 min', bestFor: 'Automated bagging', pros: ['Flows well','Fast colonization'], cons: ['Bursts if overboiled'] },
  { cat: 'substrate', title: "Master's Mix", ingredients: '50% hardwood sawdust, 50% soy hulls', thermal: '121°C @ 2.5h', bestFor: 'Oysters & lion’s mane', pros: ['High BE%','Fast flushes'], cons: ['High contamination pressure'] },
  { cat: 'substrate', title: 'Supplemented Sawdust', ingredients: '80% hardwood, 20% wheat bran', thermal: '121°C @ 2.0h', bestFor: 'Shiitake & reishi', pros: ['Stable nutrient release','Resilient'], cons: ['Lower early flush yield'] },
  { cat: 'substrate', title: 'Coir Vermiculite Gypsum', ingredients: 'Coir, vermiculite, gypsum, hot water', thermal: '75°C pasteurization', bestFor: 'Secondary decomposers', pros: ['No autoclave','Good moisture retention'], cons: ['Not ideal for wood decomposers'] },
  { cat: 'substrate', title: 'Pasteurized Wheat Straw', ingredients: 'Wheat straw + hot water', thermal: '75°C @ 2h', bestFor: 'Low-tech oyster production', pros: ['Low cost','Simple setup'], cons: ['Lower density'] },
  { cat: 'substrate', title: 'Compost Matrix', ingredients: 'Straw, horse manure, gypsum, poultry litter', thermal: 'Fermentation 14 days', bestFor: 'Agaricus bisporus', pros: ['Strong nutrient base','Commercial scale'], cons: ['Labor-intensive'] }
];

const recipeGrid = document.getElementById('recipe-grid');
let activeFilter = 'all';

function renderRecipes() {
  recipeGrid.innerHTML = '';
  recipes
    .filter((recipe) => activeFilter === 'all' || recipe.cat === activeFilter)
    .forEach((recipe) => {
      const article = document.createElement('article');
      article.className = 'recipe-card';
      article.innerHTML = `
        <div class="recipe-top">
          <h3>${recipe.title}</h3>
          <span class="recipe-tag">${recipe.cat}</span>
        </div>
        <div class="recipe-meta">
          <div><strong>Ingredients:</strong> ${recipe.ingredients}</div>
          <div><strong>Thermal:</strong> ${recipe.thermal}</div>
          <div><strong>Best for:</strong> ${recipe.bestFor}</div>
        </div>
        <ul>
          <li><strong>Pros:</strong> ${recipe.pros.join(', ')}</li>
          <li><strong>Cons:</strong> ${recipe.cons.join(', ')}</li>
        </ul>
      `;
      recipeGrid.appendChild(article);
    });
}

document.querySelectorAll('.filter-btn').forEach((button) => {
  button.addEventListener('click', () => {
    activeFilter = button.dataset.filter;
    document.querySelectorAll('.filter-btn').forEach((btn) => btn.classList.toggle('active', btn === button));
    renderRecipes();
  });
});

function switchTab(targetId) {
  document.querySelectorAll('.tab-panel').forEach((panel) => {
    panel.classList.toggle('active', panel.id === targetId);
  });
  document.querySelectorAll('.nav-btn').forEach((button) => {
    button.classList.toggle('active', button.dataset.target === targetId);
  });
}

document.querySelectorAll('.nav-btn').forEach((button) => {
  button.addEventListener('click', () => switchTab(button.dataset.target));
});

document.querySelectorAll('.link-button').forEach((button) => {
  button.addEventListener('click', () => switchTab(button.dataset.tab));
});

document.querySelectorAll('.copy-button').forEach((button) => {
  button.addEventListener('click', async () => {
    const block = document.getElementById(button.dataset.target);
    if (!block) return;
    try {
      await navigator.clipboard.writeText(block.textContent);
      const original = button.textContent;
      button.textContent = 'Copied!';
      setTimeout(() => { button.textContent = original; }, 1200);
    } catch (error) {
      button.textContent = 'Copy failed';
      setTimeout(() => { button.textContent = 'Copy Prompt'; }, 1200);
    }
  });
});

function updateCnCalculator() {
  const wood = Number(document.getElementById('input-wood').value || 0);
  const soy = Number(document.getElementById('input-soy').value || 0);
  const bran = Number(document.getElementById('input-bran').value || 0);
  const coir = Number(document.getElementById('input-coir').value || 0);
  const gypsum = Number(document.getElementById('input-gypsum').value || 0);

  document.getElementById('val-wood').textContent = `${wood.toFixed(2)} kg`;
  document.getElementById('val-soy').textContent = `${soy.toFixed(2)} kg`;
  document.getElementById('val-bran').textContent = `${bran.toFixed(2)} kg`;
  document.getElementById('val-coir').textContent = `${coir.toFixed(2)} kg`;
  document.getElementById('val-gypsum').textContent = `${gypsum.toFixed(2)} kg`;

  const carbonTotal = wood * 0.50 + soy * 0.45 + bran * 0.46 + coir * 0.48;
  const nitrogenTotal = wood * 0.001 + soy * 0.045 + bran * 0.025 + coir * 0.004;
  const totalDryWeight = wood + soy + bran + coir + gypsum;
  const waterNeeded = totalDryWeight * 1.5;
  const cnRatio = nitrogenTotal > 0 ? carbonTotal / nitrogenTotal : 0;

  document.getElementById('out-dry-weight').textContent = `${totalDryWeight.toFixed(2)} kg`;
  document.getElementById('out-water-needed').textContent = `${waterNeeded.toFixed(2)} L`;
  document.getElementById('out-cn-ratio').textContent = `${cnRatio.toFixed(1)} : 1`;

  const riskBox = document.getElementById('risk-alert-box');
  const riskTitle = document.getElementById('risk-title');
  const riskDesc = document.getElementById('risk-desc');
  const riskIcon = document.getElementById('risk-icon');

  if (cnRatio < 25) {
    riskBox.className = 'risk-box alert';
    riskIcon.textContent = '🚨';
    riskTitle.textContent = 'High Nitrogen Contamination Alert';
    riskDesc.textContent = 'C:N ratio is below 25:1. Consider higher-carbon substrate or a mandatory sterilization cycle.';
  } else if (cnRatio <= 45) {
    riskBox.className = 'risk-box success';
    riskIcon.textContent = '✅';
    riskTitle.textContent = 'Optimal Balanced C:N Ratio';
    riskDesc.textContent = 'This blend sits inside the balanced growth range for wood decomposers and many gourmet species.';
  } else {
    riskBox.className = 'risk-box warning';
    riskIcon.textContent = '⚠️';
    riskTitle.textContent = 'High Carbon Matrix';
    riskDesc.textContent = 'This is a carbon-heavy formula. Safe for pasteurization, but expect lower biological efficiency.';
  }

  if (window.cnChart) {
    window.cnChart.data.datasets[0].data = [
      Number(carbonTotal.toFixed(2)),
      Number(nitrogenTotal.toFixed(3)),
      Number(Math.max(0, totalDryWeight - carbonTotal - nitrogenTotal).toFixed(2))
    ];
    window.cnChart.update();
  }
}

document.getElementById('input-wood').addEventListener('input', updateCnCalculator);
document.getElementById('input-soy').addEventListener('input', updateCnCalculator);
document.getElementById('input-bran').addEventListener('input', updateCnCalculator);
document.getElementById('input-coir').addEventListener('input', updateCnCalculator);
document.getElementById('input-gypsum').addEventListener('input', updateCnCalculator);
document.getElementById('reset-calc').addEventListener('click', () => {
  document.getElementById('input-wood').value = 1.0;
  document.getElementById('input-soy').value = 1.0;
  document.getElementById('input-bran').value = 0.0;
  document.getElementById('input-coir').value = 0.0;
  document.getElementById('input-gypsum').value = 0.05;
  updateCnCalculator();
});

function drawDonutChart(canvas, values, colors) {
  const ctx = canvas.getContext('2d');
  const w = canvas.width;
  const h = canvas.height;
  const cx = w / 2;
  const cy = h / 2;
  const radius = Math.min(w, h) * 0.28;
  const total = values.reduce((sum, value) => sum + value, 0) || 1;

  ctx.clearRect(0, 0, w, h);
  let start = -Math.PI / 2;
  values.forEach((value, index) => {
    const slice = (value / total) * Math.PI * 2;
    ctx.beginPath();
    ctx.moveTo(cx, cy);
    ctx.arc(cx, cy, radius, start, start + slice);
    ctx.closePath();
    ctx.fillStyle = colors[index];
    ctx.fill();
    start += slice;
  });

  ctx.beginPath();
  ctx.arc(cx, cy, radius * 0.55, 0, Math.PI * 2);
  ctx.fillStyle = '#ffffff';
  ctx.fill();

  ctx.fillStyle = '#1c1917';
  ctx.textAlign = 'center';
  ctx.font = 'bold 18px Arial';
  ctx.fillText('C:N', cx, cy - 4);
  ctx.font = '12px Arial';
  ctx.fillText('Ratio', cx, cy + 14);

  const legendX = w - 110;
  const legendY = 38;
  colors.forEach((color, index) => {
    ctx.fillStyle = color;
    ctx.fillRect(legendX, legendY + index * 28, 12, 12);
    ctx.fillStyle = '#292524';
    ctx.font = '12px Arial';
    const label = ['Carbon', 'Nitrogen', 'Minerals'][index];
    ctx.fillText(label, legendX + 20, legendY + index * 28 + 11);
  });
}

function buildCnChart() {
  const canvas = document.getElementById('cn-chart');
  if (!canvas) return;
  const colors = ['#15803d', '#d97706', '#78716c'];
  window.cnChart = {
    data: { datasets: [{ data: [0.95, 0.046, 1.05] }] },
    update: () => {
      const wood = Number(document.getElementById('input-wood').value || 0);
      const soy = Number(document.getElementById('input-soy').value || 0);
      const bran = Number(document.getElementById('input-bran').value || 0);
      const coir = Number(document.getElementById('input-coir').value || 0);
      const gypsum = Number(document.getElementById('input-gypsum').value || 0);
      const carbonTotal = wood * 0.50 + soy * 0.45 + bran * 0.46 + coir * 0.48;
      const nitrogenTotal = wood * 0.001 + soy * 0.045 + bran * 0.025 + coir * 0.004;
      const totalDryWeight = wood + soy + bran + coir + gypsum;
      const values = [carbonTotal, nitrogenTotal, Math.max(0, totalDryWeight - carbonTotal - nitrogenTotal)];
      drawDonutChart(canvas, values, colors);
      window.cnChart.data.datasets[0].data = values;
    }
  };
  drawDonutChart(canvas, [0.95, 0.046, 1.05], colors);
  updateCnCalculator();
}

const telemetryScenarios = {
  optimal: {
    rh: [94, 93, 95, 92, 91, 93, 94, 92],
    co2: [620, 640, 610, 680, 710, 650, 630, 620],
    temp: [18.5, 18.3, 18.2, 18.1, 18.4, 18.6, 18.8, 18.4],
    statRh: '92.4%',
    statCo2: '650 ppm',
    statTemp: '18.2 °C'
  },
  'co2-spike': {
    rh: [92, 91, 90, 89, 88, 90, 89, 88],
    co2: [650, 820, 1150, 1580, 1820, 1600, 1300, 950],
    temp: [18.8, 19.0, 19.5, 20.1, 20.4, 20.2, 19.8, 19.4],
    statRh: '89.7%',
    statCo2: '1,280 ppm 🚨',
    statTemp: '19.6 °C'
  },
  'rh-drop': {
    rh: [90, 82, 71, 64, 60, 68, 75, 82],
    co2: [600, 610, 620, 630, 610, 600, 610, 620],
    temp: [17.8, 17.6, 17.9, 18.0, 18.2, 18.1, 18.3, 18.5],
    statRh: '74.8%',
    statCo2: '612 ppm',
    statTemp: '18.1 °C'
  }
};

function applyTelemetryScenario(name) {
  const scenario = telemetryScenarios[name];
  if (!scenario || !window.telemetryChart) return;
  window.telemetryChart.data.datasets[0].data = scenario.rh;
  window.telemetryChart.data.datasets[1].data = scenario.co2;
  window.telemetryChart.update();
  document.getElementById('stat-rh').textContent = scenario.statRh;
  document.getElementById('stat-co2').textContent = scenario.statCo2;
  document.getElementById('stat-temp').textContent = scenario.statTemp;
}

document.querySelectorAll('.scenario-btn').forEach((button) => {
  button.addEventListener('click', () => {
    document.querySelectorAll('.scenario-btn').forEach((btn) => btn.classList.toggle('active', btn === button));
    applyTelemetryScenario(button.dataset.scenario);
  });
});

function drawLineChart(canvas, labels, series1, series2) {
  const ctx = canvas.getContext('2d');
  const w = canvas.width;
  const h = canvas.height;
  const pad = { top: 16, right: 36, bottom: 40, left: 40 };
  const plotW = w - pad.left - pad.right;
  const plotH = h - pad.top - pad.bottom;

  ctx.clearRect(0, 0, w, h);
  ctx.strokeStyle = '#d6d3d1';
  ctx.lineWidth = 1;
  for (let i = 0; i <= 4; i++) {
    const y = pad.top + (plotH / 4) * i;
    ctx.beginPath();
    ctx.moveTo(pad.left, y);
    ctx.lineTo(w - pad.right, y);
    ctx.stroke();
  }

  const maxY = 2000;
  const minY = 60;
  function mapValue(value, axis) {
    const range = maxY - minY;
    const v = (value - minY) / range;
    return pad.top + plotH - v * plotH + (axis === 'co2' ? 0 : 0);
  }

  const drawSeries = (values, color, offset = 0) => {
    ctx.beginPath();
    values.forEach((value, index) => {
      const x = pad.left + (plotW / (values.length - 1)) * index;
      const y = pad.top + plotH - ((value - minY) / (maxY - minY)) * plotH;
      if (index === 0) ctx.moveTo(x, y);
      else ctx.lineTo(x, y);
    });
    ctx.strokeStyle = color;
    ctx.lineWidth = offset === 0 ? 2 : 2.2;
    ctx.setLineDash(offset === 0 ? [] : [6, 6]);
    ctx.stroke();
    ctx.setLineDash([]);
  };

  drawSeries(series1, '#15803d', 0);
  drawSeries(series2, '#d97706', 1);

  ctx.fillStyle = '#292524';
  ctx.font = '12px Arial';
  labels.forEach((label, index) => {
    const x = pad.left + (plotW / (labels.length - 1)) * index;
    ctx.fillText(label, x - 14, h - 12);
  });

  ctx.fillText('RH %', 10, 18);
  ctx.fillText('CO2 ppm', w - 72, 18);
}

function buildTelemetryChart() {
  const canvas = document.getElementById('telemetry-chart');
  if (!canvas) return;
  const labels = ['00:00', '03:00', '06:00', '09:00', '12:00', '15:00', '18:00', '21:00'];
  const scenario = telemetryScenarios.optimal;
  drawLineChart(canvas, labels, scenario.rh, scenario.co2);
  window.telemetryChart = {
    data: { datasets: [{ data: scenario.rh }, { data: scenario.co2 }] },
    update: () => {
      const active = document.querySelector('.scenario-btn.active');
      const name = active ? active.dataset.scenario : 'optimal';
      const current = telemetryScenarios[name] || telemetryScenarios.optimal;
      drawLineChart(canvas, labels, current.rh, current.co2);
      window.telemetryChart.data.datasets[0].data = current.rh;
      window.telemetryChart.data.datasets[1].data = current.co2;
    }
  };
  applyTelemetryScenario('optimal');
}

function drawVisionCanvas() {
  const canvas = document.getElementById('vision-canvas');
  if (!canvas) return;
  const ctx = canvas.getContext('2d');
  const gradient = ctx.createLinearGradient(0, 0, canvas.width, canvas.height);
  gradient.addColorStop(0, '#d6d3d1');
  gradient.addColorStop(1, '#e7e5e4');
  ctx.fillStyle = gradient;
  ctx.fillRect(0, 0, canvas.width, canvas.height);

  ctx.fillStyle = 'rgba(22, 163, 74, 0.12)';
  ctx.fillRect(60, 70, 160, 180);
  ctx.fillStyle = 'rgba(22, 163, 74, 0.22)';
  ctx.fillRect(220, 100, 190, 140);

  ctx.strokeStyle = '#dc2626';
  ctx.lineWidth = 3;
  ctx.strokeRect(105, 130, 90, 70);
  ctx.strokeRect(260, 150, 110, 80);

  ctx.fillStyle = '#166534';
  ctx.font = 'bold 18px Arial';
  ctx.fillText('Trichoderma-like mold', 90, 235);
  ctx.fillText('Patch cluster', 245, 260);
}

function drawLineSeries(canvas, points, color, fill = false) {
  const ctx = canvas.getContext('2d');
  const w = canvas.width;
  const h = canvas.height;
  const pad = { top: 20, right: 20, bottom: 32, left: 38 };
  const plotW = w - pad.left - pad.right;
  const plotH = h - pad.top - pad.bottom;

  ctx.clearRect(0, 0, w, h);
  ctx.strokeStyle = '#e7e5e4';
  ctx.beginPath();
  for (let i = 0; i <= 4; i++) {
    const y = pad.top + (plotH / 4) * i;
    ctx.moveTo(pad.left, y);
    ctx.lineTo(w - pad.right, y);
  }
  ctx.stroke();

  ctx.beginPath();
  points.forEach((value, index) => {
    const x = pad.left + (plotW / (points.length - 1)) * index;
    const y = pad.top + plotH - ((value - 50) / 50) * plotH;
    if (index === 0) ctx.moveTo(x, y);
    else ctx.lineTo(x, y);
  });
  ctx.strokeStyle = color;
  ctx.lineWidth = 2;
  ctx.stroke();

  if (fill) {
    const lastX = pad.left + (plotW / (points.length - 1)) * (points.length - 1);
    const lastY = pad.top + plotH - ((points[points.length - 1] - 50) / 50) * plotH;
    ctx.lineTo(lastX, h - pad.bottom);
    ctx.lineTo(pad.left, h - pad.bottom);
    ctx.closePath();
    ctx.fillStyle = color + '44';
    ctx.fill();
  }

  ctx.fillStyle = '#292524';
  ctx.font = '12px Arial';
  ['P1', 'P2', 'P3', 'P4', 'P5', 'P6'].forEach((label, index) => {
    const x = pad.left + (plotW / 5) * index;
    ctx.fillText(label, x - 8, h - 10);
  });
}

function buildSenescenceChart() {
  const canvas = document.getElementById('senescence-chart');
  if (!canvas) return;
  drawLineSeries(canvas, [96, 93, 89, 84, 76, 68], '#166534', true);
}

function recalcPlanner() {
  const targetYield = Number(document.getElementById('yield-kg').value || 0);
  const bePercent = Number(document.getElementById('be-percent').value || 0);
  const substrateWet = Number(document.getElementById('substrate-wet').value || 0);
  const spawnRatio = Number(document.getElementById('spawn-ratio').value || 0);

  const drySubstrate = targetYield / (bePercent / 100) * (1 / substrateWet);
  const spawnNeeded = drySubstrate * spawnRatio;
  const lcVolume = Math.max(0.5, drySubstrate * 0.02);
  const plates = Math.max(3, Math.ceil(spawnNeeded / 6));
  const runs = Math.max(1, Math.ceil(drySubstrate / 120));

  document.getElementById('out-substrate').textContent = `${drySubstrate.toFixed(0)} kg wet`;
  document.getElementById('out-spawn').textContent = `${spawnNeeded.toFixed(0)} kg`;
  document.getElementById('out-lc').textContent = `${lcVolume.toFixed(0)} mL`;
  document.getElementById('out-plates').textContent = `${plates} plates`;
  document.getElementById('out-runs').textContent = `${runs} runs`;
}

document.getElementById('recalc-planner').addEventListener('click', recalcPlanner);

renderRecipes();
buildCnChart();
buildTelemetryChart();
drawVisionCanvas();
buildSenescenceChart();
recalcPlanner();
