$(document).ready(function () {
    $('.btn-salvar').click(function () {
        var form = $('#salvarCategoria')[0]; // Obtendo o formulário real
        var formData = new FormData(form);

        if (form.checkValidity() === false) {
            $('#salvarCategoria').addClass('was-validated');
        } else {
            $.ajax({
                url: '/Categoria/Create',
                type: 'POST',
                data: formData,
                contentType: false, // Não deixe o jQuery definir o tipo de conteúdo
                processData: false, // Impede o jQuery de processar o FormData
                success: function (response) {
                    if (response.success) {
                        let timerInterval;
                        Swal.fire({
                            icon: 'success',
                            title: response.message,
                            html: '<b>4</b> segundos.',
                            timer: 4000, // 5 segundos
                            timerProgressBar: true,
                            allowOutsideClick: false,
                            didOpen: () => {
                                const b = Swal.getHtmlContainer().querySelector('b');
                                timerInterval = setInterval(() => {
                                    b.textContent = Math.ceil(Swal.getTimerLeft() / 1000); // Atualiza o cronômetro
                                }, 1000);
                            },
                            willClose: () => {
                                clearInterval(timerInterval); // Limpa o intervalo quando o modal fechar
                            }
                        }).then((result) => {
                            // Após fechar o Swal, executa as seguintes ações:
                            $('#modalCadastrarCategoria').modal('hide');
                            $('#salvarCategoria')[0].reset();
                            $('#salvarCategoria').removeClass('was-validated');
                            location.reload();  // Recarrega a página após o modal fechar
                            $('#modalCadastrarCategoria').modal('hide');
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

    $('.btn-salvar-sub').click(function () {
        var form = $('#salvarSubCategoria')[0]; // Obtendo o formulário real
        var formData = new FormData(form);

        if (form.checkValidity() === false) {
            $('#salvarSubCategoria').addClass('was-validated');
        } else {
            $.ajax({
                url: '/SubCategoria/Create',
                type: 'POST',
                data: formData,
                contentType: false,
                processData: false,
                success: function (response) {
                    if (response.success) {
                        let timerInterval;
                        Swal.fire({
                            icon: 'success',
                            title: response.message,
                            html: '<b>4</b> segundos.',
                            timer: 4000,
                            timerProgressBar: true,
                            allowOutsideClick: false,
                            didOpen: () => {
                                const b = Swal.getHtmlContainer().querySelector('b');
                                timerInterval = setInterval(() => {
                                    b.textContent = Math.ceil(Swal.getTimerLeft() / 1000);
                                }, 1000);
                            },
                            willClose: () => {
                                clearInterval(timerInterval);
                            }
                        }).then((result) => {
                            $('#modalCadastrarSubCategoria').modal('hide');
                            location.reload();
                            $('#modalCadastrarSubCategoria').modal('hide');
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

    $('#categoriaId').on("change", function () {      
        var cat = parseInt($(this).val());
        $("#_FkCategoria").val(cat);
        console.log(cat);
        $.ajax({
            url: '/SubCategoria/Detalhes',
            type: 'POST',
            data: { id: cat },
            success: function (response) {
                // Exibe a resposta no HTML
                $("#tableDetalhes").html(response);
            },
            error: function (jqXHR, textStatus, errorThrown) {
                Swal.fire({
                    icon: 'info',
                    title: 'Sem SubCategorias',                  
                    html: '<b>2</b> segundos.',
                    timer: 2000,
                    timerProgressBar: true,
                    allowOutsideClick: false,
                    didOpen: () => {
                        const b = Swal.getHtmlContainer().querySelector('b');
                        timerInterval = setInterval(() => {
                            b.textContent = Math.ceil(Swal.getTimerLeft() / 1000);
                        }, 1000);
                    },
                    willClose: () => {
                        clearInterval(timerInterval);
                    }
                }).then((result) => {
                  
                });               
            }
        });

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