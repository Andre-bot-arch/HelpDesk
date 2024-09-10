$(document).ready(function () {
    function readURL(input, previewId, callback) {
        if (input.files && input.files[0]) {
            var reader = new FileReader();
            reader.onload = function (e) {
                $(previewId).attr('src', e.target.result).show();
                if (callback) {
                    callback(e.target.result); // Envia a imagem em Base64 para o callback
                }
            };
            reader.readAsDataURL(input.files[0]);
        }
    }

    // Variáveis para armazenar a imagem Base64 e a extensão do arquivo
    var imagemBase64 = '';
    var extensaoArquivo = '';

    // Função para remover o cabeçalho e extrair a extensão do arquivo
    function processBase64(base64) {
        // Dividir a string base64 no cabeçalho e no conteúdo
        var base64Parts = base64.split(',');
        var mimeType = base64Parts[0].match(/:(.*?);/)[1]; // Extrai o mime type, ex: "image/png"
        var pureBase64 = base64Parts[1]; // Essa é a parte pura em Base64

        // Verificar a extensão com base no mime type
        var extensao = mimeType.split('/')[1]; // Exemplo: "png", "jpeg"

        return {
            base64: pureBase64,
            extensao: extensao
        };
    }

    // Pré-visualizar imagem no modal de Cadastrar
    $("#imagemBase64Cadastrar").change(function () {
        readURL(this, '#previewImagemCadastrar', function (base64) {
            var result = processBase64(base64);
            imagemBase64 = result.base64; // Armazena o Base64 da imagem sem o cabeçalho
            extensaoArquivo = result.extensao; // Armazena a extensão do arquivo
        });
    });

    // Pré-visualizar imagem no modal de Editar
    $("#imagemBase64Editar").change(function () {
        readURL(this, '#previewImagemEditar', function (base64) {
            var result = processBase64(base64);
            imagemBase64 = result.base64; // Armazena o Base64 da imagem sem o cabeçalho
            extensaoArquivo = result.extensao; // Armazena a extensão do arquivo
        });
    });

    // Submit para o formulário de Cadastrar
    $('#btnSalvar').click(function () {
        var form = $('#formCadastrarPlataforma')[0]; // Obtendo o formulário real
        var formData = new FormData(form); // Criando o FormData para permitir o envio do arquivo

        // Adiciona o Base64 da imagem e a extensão ao formData
        formData.append('imagemBase64', imagemBase64);
        formData.append('ExtensaoArquivo', extensaoArquivo);

        if (form.checkValidity() === false) {
            $('#formCadastrarPlataforma').addClass('was-validated');
        } else {
            $.ajax({
                url: '/Plataforma/Create',
                type: 'POST',
                data: formData,
                contentType: false, // Não deixe o jQuery definir o tipo de conteúdo
                processData: false, // Impede o jQuery de processar o FormData
                success: function (response) {
                    if (response.success) {
                        Swal.fire({
                            icon: 'success',
                            title: 'Sucesso!',
                            text: response.message,
                            confirmButtonText: 'OK'
                        }).then((result) => {
                            if (result.isConfirmed) {
                                $('#modalCadastrarPlataforma').modal('hide');
                                $('#formCadastrarPlataforma')[0].reset();
                                $('#formCadastrarPlataforma').removeClass('was-validated');
                                location.reload();
                            }
                        });
                    } else {
                        Swal.fire({
                            icon: 'error',
                            title: 'Erro!',
                            text: response.message,
                            confirmButtonText: 'OK'
                        });
                    }
                },
                error: function () {
                    Swal.fire({
                        icon: 'error',
                        title: 'Erro!',
                        text: 'Ocorreu um erro inesperado.',
                        confirmButtonText: 'OK'
                    });
                }
            });
        }
    });

    // Função para preencher o formulário de edição com dados e a imagem atual
    $(document).on('click', '.btn-edit', function () {
        var plataformaId = $(this).data('id'); // Pegar o ID da plataforma

        $.ajax({
            url: '/Plataforma/Detalhes',
            type: 'GET',
            data: { id: plataformaId },
            success: function (response) {
                if (response.success) {
                    // Preencher os campos do formulário de edição
                    $('#nomePlataforma').val(response.data.nomePlataforma);
                    $('#descricao').val(response.data.descricao);
                    $('#plataformaId').val(response.data.plataformaId);

                    // Mostrar a imagem atual no modal de edição (se houver)
                    if (response.data.imagemUrl) {
                        $('#previewImagemEditar').attr('src', response.data.imagemUrl).show();
                    } else {
                        $('#previewImagemEditar').hide(); // Se não houver imagem, esconda o preview
                    }

                    $('#modalEditarPlataforma').modal('show');
                } else {
                    Swal.fire({
                        icon: 'error',
                        title: 'Erro!',
                        text: response.message,
                        confirmButtonText: 'OK'
                    });
                }
            },
            error: function () {
                Swal.fire({
                    icon: 'error',
                    title: 'Erro!',
                    text: 'Ocorreu um erro ao buscar os detalhes.',
                    confirmButtonText: 'OK'
                });
            }
        });
    });

    // Submit para o formulário de Editar
    $('#btnEditar').click(function () {
        var form = $('#formEditarPlataforma')[0]; // Obtendo o formulário real
        var formData = new FormData(form); // Criando o FormData para enviar arquivo

        // Adiciona o Base64 da imagem e a extensão ao formData
        formData.append('imagemBase64', imagemBase64);
        formData.append('ExtensaoArquivo', extensaoArquivo);

        if (form.checkValidity() === false) {
            $('#formEditarPlataforma').addClass('was-validated');
        } else {
            $.ajax({
                url: '/Plataforma/Edit',
                type: 'POST',
                data: formData,
                contentType: false, // Não definir o tipo de conteúdo
                processData: false, // Não processar o FormData
                success: function (response) {
                    if (response.success) {
                        Swal.fire({
                            icon: 'success',
                            title: 'Sucesso!',
                            text: response.message,
                            confirmButtonText: 'OK'
                        }).then((result) => {
                            if (result.isConfirmed) {
                                $('#modalEditarPlataforma').modal('hide');
                                $('#formEditarPlataforma')[0].reset();
                                $('#formEditarPlataforma').removeClass('was-validated');
                                location.reload();
                            }
                        });
                    } else {
                        Swal.fire({
                            icon: 'error',
                            title: 'Erro!',
                            text: response.message,
                            confirmButtonText: 'OK'
                        });
                    }
                },
                error: function () {
                    Swal.fire({
                        icon: 'error',
                        title: 'Erro!',
                        text: 'Ocorreu um erro inesperado.',
                        confirmButtonText: 'OK'
                    });
                }
            });
        }
    });
});