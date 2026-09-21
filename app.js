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

drawVisionCanvas();

recalcPlanner();
