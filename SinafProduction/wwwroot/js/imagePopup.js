const images = document.getElementsByClassName("popup-image");

for (const image of images) {
    image.addEventListener("click", () => {
        image.classList.toggle("popup-image--show", true);
    })
}