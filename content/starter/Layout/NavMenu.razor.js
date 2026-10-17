const menu = document.querySelector('[data-nav-menu]');
const toggle = menu.querySelector('button');
const navigation = menu.querySelector('nav');

function setExpanded(expanded) {
    toggle.setAttribute('aria-expanded', String(expanded));
    navigation.dataset.collapsed = String(!expanded);
}

setExpanded(false);
toggle.hidden = false;

toggle.addEventListener('click', () => {
    setExpanded(toggle.getAttribute('aria-expanded') !== 'true');
});
