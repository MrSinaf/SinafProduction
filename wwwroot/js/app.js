document.addEventListener('DOMContentLoaded', updateNav);
document.addEventListener('scroll', updateNav);

function updateNav() {
    const navElement = document.querySelector('header > nav');
    if (navElement != null)
        navElement .classList.toggle('fixed', window.scrollY > 15)
}