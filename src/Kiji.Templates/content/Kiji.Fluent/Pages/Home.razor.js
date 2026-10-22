await customElements.whenDefined('fluent-button');

const cards = [...document.querySelectorAll('[data-resource]')];
const filters = [...document.querySelectorAll('[data-category-filter]')];
const search = document.getElementById('resource-search');
const status = document.querySelector('[data-result-count]');
const empty = document.querySelector('[data-empty]');
let category = 'All';

function filterResources() {
    const query = search.value.trim().toLowerCase();
    let count = 0;
    for (const card of cards) {
        card.hidden = !(category === 'All' || card.dataset.category === category)
            || !card.textContent.toLowerCase().includes(query);
        if (!card.hidden) count++;
    }
    status.textContent = `${count} ${count === 1 ? 'resource' : 'resources'} to explore`;
    empty.hidden = count !== 0;
    for (const button of filters) {
        const active = button.dataset.categoryFilter === category;
        button.setAttribute('aria-pressed', String(active));
        button.setAttribute('appearance', active ? 'primary' : 'outline');
    }
}

filters.forEach(button => button.addEventListener('click', () => {
    category = button.dataset.categoryFilter;
    filterResources();
}));
search.addEventListener('input', filterResources);
document.querySelector('[data-reset]').addEventListener('click', () => {
    category = 'All';
    search.value = '';
    filterResources();
    search.focus();
});

document.querySelector('.library-tools').hidden = false;
