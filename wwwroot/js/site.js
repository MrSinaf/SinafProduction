const nav = document.querySelector("nav");

// TODO : Voir si je dois garder ça
const inputImages = document.querySelectorAll(".input-image--script");
for (const input of inputImages) {
    input.addEventListener("change", event => {
        const file = event.target.files[0];
        
        const reader = new FileReader()
        reader.onload = readerEvent => {
            const label = input.querySelector("label");
            const p = label.querySelector("p") ?? document.createElement("p");
            const element = label.querySelector("label > img") ?? document.createElement("img");
            
            element.src = readerEvent.target.result.toString();
            p.innerText = file.name;
            
            label.append(element);
            label.append(p);
        }
        reader.readAsDataURL(file);
    })
}

function navButtonClick() {
    nav.classList.toggle("nav--open");
}