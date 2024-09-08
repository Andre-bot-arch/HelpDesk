$(document).ready(function () {
    $('#btnSalvar').click(function () {
        var form = $('#formCadastrarPlataforma')[0];

        if (form.checkValidity() === false) {
            $('#formCadastrarPlataforma').addClass('was-validated');
        } else {
            var formData = new FormData(form); // Captura o formulário inteiro, inclusive campos padrões

            var imageBase64 = $('#preview').attr('src').split(',')[1]; // Remove o prefixo do Base64
            formData.append('ImagemBase64', imageBase64); // Passa a imagem Base64

            // Continua com a lógica do envio
            formData.append('ExtensaoArquivo', $('#fileExtension').val());

            $.ajax({
                url: '/Plataforma/Create',
                type: 'POST',
                data: formData,
                contentType: false, // Desabilitado para permitir o envio de FormData
                processData: false, // Evita que o jQuery faça o processamento dos dados
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
                                location.reload(); // Recarrega a página
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

    $(document).on('click', '.btn-edit', function () {
        var plataformaId = $(this).data('id'); // Get the ID from the button's data attribute

        $.ajax({
            url: '/Plataforma/Detalhes', // URL da action na controller
            type: 'GET',
            data: { id: plataformaId },
            success: function (response) {
                // Manipule a resposta e faça o que for necessário
                // Suponha que a resposta seja um JSON com os detalhes da plataforma
                if (response.success) {
                    // Preencher os campos do formulário com os dados recebidos
                    $('#nomePlataforma').val(response.data.nomePlataforma);
                    $('#descricao').val(response.data.descricao);
                    $('#plataformaId').val(response.data.plataformaId);
                    // Adicione aqui a lógica para manipular caminhoImagem e extensaoImagem
                    var caminhoImagem = response.data.caminhoImagem;

                    // Verifica se a imagem com o ID 'preview' já existe
                    var existingImg = $('#image-container').find('#preview');

                    if (existingImg.length) {
                        if (caminhoImagem) {
                            // Atualiza o src da imagem existente
                            existingImg.attr('src', caminhoImagem);
                        } else {
                            caminhoImagem = 'https://www2.camara.leg.br/atividade-legislativa/comissoes/comissoes-permanentes/cindra/imagens/sem.jpg.gif'
                            existingImg.attr('src', caminhoImagem);
                        }
                    } else {
                        if (!caminhoImagem) {
                            caminhoImagem = 'https://www2.camara.leg.br/atividade-legislativa/comissoes/comissoes-permanentes/cindra/imagens/sem.jpg.gif'
                        }
                        // Cria um novo elemento de imagem
                        var img = $('<img>', {
                            src: caminhoImagem,
                            alt: 'Sem imagem!',
                            class: 'img-fluid',
                            id: 'preview'
                        });

                        // Adiciona a nova imagem dentro da div com ID 'image-container'
                        $('#image-container').append(img);
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

    $('#btnEditar').click(function () {
        var form = $('#formEditarPlataforma');

        if (form[0].checkValidity() === false) {
            form.addClass('was-validated');
        } else {
            var formData = new FormData(form); // Captura o formulário inteiro, inclusive campos padrões

            var imageBase64 = $('#preview').attr('src').split(',')[1]; // Remove o prefixo do Base64
            formData.append('ImagemBase64', imageBase64); // Passa a imagem Base64

            // Continua com a lógica do envio
            formData.append('ExtensaoArquivo', $('#fileExtension').val());

            $.ajax({
                url: '/Plataforma/Edit',
                type: 'POST',
                data: formData,
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
                                form[0].reset();
                                form.removeClass('was-validated');
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