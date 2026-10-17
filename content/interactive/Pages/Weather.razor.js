const weather = document.querySelector('[data-weather]');
const status = weather.querySelector('[role="status"]');
const table = weather.querySelector('.table-scroll');
const body = weather.querySelector('tbody');

status.textContent = 'Loading...';
status.hidden = false;

try {
    const response = await fetch(weather.dataset.url);
    if (!response.ok) {
        throw new Error(`Weather request failed: ${response.status}`);
    }

    const forecasts = await response.json();
    for (const forecast of forecasts) {
        const row = document.createElement('tr');
        const date = new Date(`${forecast.date}T00:00:00`).toLocaleDateString();
        const temperatureF = 32 + Math.trunc(forecast.temperatureC / 0.5556);
        for (const value of [date, forecast.temperatureC, temperatureF, forecast.summary]) {
            const cell = document.createElement('td');
            cell.textContent = value;
            row.append(cell);
        }
        body.append(row);
    }

    status.hidden = true;
    table.hidden = false;
} catch {
    status.textContent = 'Unable to load the weather forecast.';
}
