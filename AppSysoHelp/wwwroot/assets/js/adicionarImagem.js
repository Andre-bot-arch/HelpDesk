$(document).ready(function () {
    const dropZone = document.getElementById('drop-zone');
    const fileInput = document.getElementById('file-input');
    const preview = document.getElementById('preview');
    const errorMessage = document.getElementById('error-message');
    const imageBase64 = document.getElementById('image-base64');
    const fileExtension = document.getElementById('fileExtension');
    const MAX_FILE_SIZE = 3 * 1024 * 1024; // 3MB

    dropZone.addEventListener('dragover', (e) => {
        e.preventDefault();
        dropZone.classList.add('dragover');
    });

    dropZone.addEventListener('dragleave', () => {
        dropZone.classList.remove('dragover');
    });

    dropZone.addEventListener('drop', (e) => {
        e.preventDefault();
        dropZone.classList.remove('dragover');
        const files = e.dataTransfer.files;
        if (files.length > 0) {
            handleFiles(files);
        }
    });

    fileInput.addEventListener('change', (e) => {
        const files = e.target.files;
        if (files.length > 0) {
            handleFiles(files);
        }
    });

    function handleFiles(files) {
        const file = files[0];
        const fileTypes = ['image/jpeg', 'image/jpg', 'image/png'];

        if (file.size > MAX_FILE_SIZE) {
            errorMessage.textContent = 'O arquivo é muito grande. O tamanho máximo permitido é 3MB.';
            return;
        }

        if (fileTypes.includes(file.type)) {
            const reader = new FileReader();
            reader.onload = (e) => {
                console.log('File loaded successfully');
                console.log('File type:', file.type);
                console.log('File name:', file.name);

                preview.src = e.target.result;
                errorMessage.textContent = ''; // Clear previous error messages

                $('#imageBase64').val(e.target.result.split(',')[1]); // Set base64 string in hidden field
                $('#fileExtension').val(file.name.split('.').pop().toLowerCase()); // Set file extension in hidden field
            };
            reader.readAsDataURL(file);
        } else {
            errorMessage.textContent = 'Apenas arquivos .jpg, .jpeg e .png são aceitos.';
        }
    }
});