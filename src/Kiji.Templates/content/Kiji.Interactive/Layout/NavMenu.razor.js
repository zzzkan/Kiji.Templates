const menu = document.querySelector('[data-nav-menu]');
const toggle = menu.querySelector('button');
const navigation = menu.querySelector('.nav-scrollable');
const mobile = window.matchMedia('(max-width: 640.98px)');

function setExpanded(expanded) {
    toggle.setAttribute('aria-expanded', String(expanded));
    navigation.classList.toggle('collapse', !expanded);
}

setExpanded(!mobile.matches);
toggle.hidden = false;
mobile.addEventListener('change', () => setExpanded(!mobile.matches));

toggle.addEventListener('click', () => {
    setExpanded(toggle.getAttribute('aria-expanded') !== 'true');
});

navigation.addEventListener('click', (event) => {
    if (mobile.matches && event.target.closest('a')) {
        setExpanded(false);
    }
});
