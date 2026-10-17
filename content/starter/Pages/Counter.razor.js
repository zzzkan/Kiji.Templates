const counter = document.querySelector('[data-counter]');
const count = counter.querySelector('[data-count]');
const button = counter.querySelector('button');
let currentCount = 0;

button.disabled = false;
button.addEventListener('click', () => {
    currentCount += 1;
    count.textContent = String(currentCount);
});
