function openModal(imagePath) {
    var modal = document.getElementById("imageModal");
    var modalImg = document.getElementById("enlargedImg");
    modal.style.display = "flex";
    modalImg.src = imagePath;
    modalImg.style.width = "50%";
}

function closeModal() {
    var modal = document.getElementById("imageModal");
    modal.style.display = "none";
}