const imageInput = document.getElementById("image");
const imageList = document.getElementById("image-list");
const iconInput = document.getElementById("icon");
const blogId = document.getElementById("id").value;


fetch(`/api/blogs/${blogId}/images`, {
    method: "GET",
    credentials: "same-origin"
}).then(async response => {
    if (!response.ok)
        return;

    let resultJSON = await response.json();

    for (const image of resultJSON["images"]) {
        createImageInList(image);
    }
});

imageInput.addEventListener("change", async event => createImageInList(await sendImage(event.target.files[0])));
iconInput.addEventListener("change", async event => await sendIcon(event.target.files[0]));

// fonctions :
async function clearIcon() {
    if (confirm("Voulez-vous réellement supprimer l'icône (cette action est irréversible).") === false)
        return;

    const response = await fetch(`/api/blogs/${blogId}/icon`, {
        method: "DELETE",
        credentials: "same-origin"
    });

    if (!response.ok)
        throw new Error();

    const imgIcon = iconInput.querySelector("img");
    imgIcon.src = `/blogs/${blogId}/icon.png?v=${new Date().getTime()}`;
}

async function sendIcon(icon) {
    let formData = new FormData();
    formData.append("icon", icon);

    const response = await fetch(`/api/blogs/${blogId}/icon`, {
        method: "POST",
        body: formData,
        credentials: "same-origin"
    });

    if (!response.ok)
        throw new Error();

    const imgIcon = iconInput.querySelector("img");
    imgIcon.src = `/blogs/${blogId}/icon.png?v=${new Date().getTime()}`;
}

async function sendImage(file) {
    if (!file) {
        console.error("Aucun fichier sélectionné");
        return;
    }

    const formData = new FormData();
    formData.append("image", file);

    const response = await fetch(`/api/blogs/${blogId}/images`, {
        method: "POST",
        body: formData,
        credentials: "same-origin"
    });

    if (!response.ok)
        throw new Error();

    return await response.json();
}

function createImageInList(image) {

    const li = document.createElement("li");
    li.className = "input-image__preview";

    const img = document.createElement("img");
    img.src = image["path"];
    li.append(img);

    const deleteButton = document.createElement("button");
    deleteButton.className = "input-image__preview__button-close";
    deleteButton.innerText = "X";
    deleteButton.type = "button";
    deleteButton.onclick = async () => {
        if (confirm("Voulez-vous vraiment supprimer le fichier, cette action est irréversible !") === false)
            return;

        const response = await fetch(`/api/blogs/${blogId}/images/${encodeURIComponent(image["name"])}`, {
            method: "DELETE",
            credentials: "same-origin"
        });

        if (response.ok) {
            li.remove();
        } else {
            throw new Error(`Erreur : ${response.status}`);
        }
    }
    li.append(deleteButton);

    const copyButton = document.createElement("button");
    copyButton.className = "input-image__preview__button-copy";
    copyButton.innerText = "Copier";
    copyButton.type = "button";
    copyButton.onclick = async () => {
        await navigator.clipboard.writeText(`![](${image["path"]})`);
    }
    li.append(copyButton);

    imageList.append(li);
}