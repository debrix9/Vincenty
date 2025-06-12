// Inizializza mappa con Leaflet
const map = L.map('map').setView([45.0, 9.0], 10);

L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
  maxZoom: 19,
  attribution: '© OpenStreetMap contributors'
}).addTo(map);

const marker = L.marker([45.0, 9.0]).addTo(map);

// Riferimenti input e label
const inputModeSwitch = document.getElementById('inputModeSwitch');
const input1 = document.getElementById('input1');
const input2 = document.getElementById('input2');
const input3 = document.getElementById('input3');
const input4 = document.getElementById('input4');
const generateBtn = document.getElementById('generateBtn');
const checkbox = document.getElementById('inputModeSwitch');
const label = document.getElementById('switchLabel');

function updateLabels() {
  if (inputModeSwitch.checked) {

    input1.placeholder = "Enter Latitude";
    input2.placeholder = "Enter Longitude";
    input3.style.display = "none";
    input4.style.display = "none";
  } else {
    input1.placeholder = "Enter Bearing";
    input2.placeholder = "Enter Range";
    input3.style.display = "block";
    input4.style.display = "block";
  }
}

checkbox.addEventListener('change', () => {
  if (checkbox.checked)
  {
    label.textContent = "Inverse Problem";
  }
  else
  {
    label.textContent = "Direct Problem";
  }
});


inputModeSwitch.addEventListener('change', updateLabels);
updateLabels();

generateBtn.addEventListener('click', () => {
  if (inputModeSwitch.checked) {
    // Modalità Coordinate
    const lat = parseFloat(input1.value);
    const lon = parseFloat(input2.value);
    if (!isNaN(lat) && !isNaN(lon)) {
      map.setView([lat, lon], 10);
      marker.setLatLng([lat, lon]);
      console.log(`Moved map to coordinates: (${lat}, ${lon})`);
    } else {
      alert("Inserisci coordinate valide!");
    }
  } else {
    // Modalità Bearing & Range
    const bearing = parseFloat(input1.value);
    const range = parseFloat(input2.value);
    if (!isNaN(bearing) && !isNaN(range)) {
      console.log(`Bearing: ${bearing}, Range: ${range}`);
      alert(`Modalità Bearing & Range selezionata.\nBearing: ${bearing}\nRange: ${range}`);
    } else {
      alert("Inserisci bearing e range validi!");
    }
  }
});
