$(document).ready(function () {
    $('#btnSalvar').click(function () {
        var form = $('#formCadastrarPlataforma');

        if (form[0].checkValidity() === false) {
            form.addClass('was-validated');
        } else {
            var formData = form.serialize();

            $.ajax({
                url: '/Plataforma/Create',
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
            var formData = form.serialize();

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