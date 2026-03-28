window.getInputValueByQuery = (query) => {
    const el = document.querySelector(query);
    return el ? el.value : "";
};

window.clearInputValueByQuery = (query) => {
    const el = document.querySelector(query);
    if (el) el.value = "";
};

window.getQuillContent = (id) => {
    const el = document.getElementById(id);
    if (!el) return "";
    return el.querySelector('.ql-editor').innerHTML;
};

window.setQuillContent = (id, html) => {
    const el = document.getElementById(id);
    if (!el) return;
    const editor = el.querySelector('.ql-editor');
    if (editor) editor.innerHTML = html || "";
};

// Initialize Quill when component renders
window.initQuill = (id) => {
    if (!document.getElementById(id)) return;
    new Quill('#' + id, {
        theme: 'snow',
        modules: {
            toolbar: [
                [{ 'header': [1, 2, 3, false] }],
                ['bold', 'italic', 'underline', 'strike'],
                ['link', 'blockquote', 'code-block'],
                [{ 'list': 'ordered'}, { 'list': 'bullet' }],
                ['clean']
            ]
        }
    });
};
// Reveal elements on scroll
window.setupRevealOnScroll = () => {
    const reveals = document.querySelectorAll('.reveal');
    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('active');
            }
        });
    }, { threshold: 0.15 });

    reveals.forEach(reveal => observer.observe(reveal));
};

// Map Analytics
let userMap;
let mapMarkers = [];

window.initMap = () => {
    if (userMap) {
        userMap.invalidateSize();
        return;
    }
    const mapEl = document.getElementById('map');
    if (!mapEl) return;
    
    // Using CartoDB Dark Matter tiles for a premium look
    userMap = L.map('map').setView([20, 0], 2);
    L.tileLayer('https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}{r}.png', {
        attribution: '© OpenStreetMap'
    }).addTo(userMap);
};

window.updateMapMarkers = (usersJson) => {
    if (!userMap) {
        window.initMap();
    }
    if (!userMap) return;
    
    const users = JSON.parse(usersJson);
    
    // Clear old markers
    mapMarkers.forEach(m => userMap.removeLayer(m));
    mapMarkers = [];
    
    users.forEach(u => {
        if (u.IsLocated) {
            const marker = L.circleMarker([u.Latitude, u.Longitude], {
                radius: 8,
                fillColor: "#0d6efd",
                color: "#fff",
                weight: 2,
                opacity: 1,
                fillOpacity: 0.8
            }).addTo(userMap);
            
            marker.bindTooltip(`<b>${u.City}, ${u.Country}</b><br>Viewing: /${u.CurrentUrl || ''}`);
            mapMarkers.push(marker);
        }
    });
};

// Analytics Charts
let trafficChart;

window.initTrafficChart = (labelsJson, dataJson) => {
    const ctx = document.getElementById('trafficChart');
    if (!ctx) return;
    
    if (trafficChart) {
        trafficChart.destroy();
    }
    
    const labels = JSON.parse(labelsJson);
    const data = JSON.parse(dataJson);
    
    trafficChart = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [{
                label: 'Visitors',
                data: data,
                backgroundColor: 'rgba(255, 184, 0, 0.2)',
                borderColor: '#ffb800',
                borderWidth: 2,
                borderRadius: 8,
                hoverBackgroundColor: 'rgba(255, 184, 0, 0.4)'
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: { display: false }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    grid: { color: 'rgba(255,255,255,0.05)' },
                    ticks: { color: 'rgba(255,255,255,0.5)' }
                },
                x: {
                    grid: { display: false },
                    ticks: { color: 'rgba(255,255,255,0.5)' }
                }
            }
        }
    });
};
