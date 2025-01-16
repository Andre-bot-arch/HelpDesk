$(document).ready(function () {
    $("#FkSetores_").change(function (e) {
        if ($(this).val() == "6") {
            $("#tempo").show(); // Exibe a div
        } else {
            $("#tempo").hide(); // Oculta a div
        }
    });

    $('.btn-salvar').click(function (e) {
        e.preventDefault();

        var form = $('#salvarChamado')[0];
        var formData = new FormData(form);

        Swal.fire({
            title: 'Tem certeza?',
            text: "Deseja salvar este chamado agora?",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: 'Sim, salvar!',
            cancelButtonText: 'Cancelar'
        }).then((result) => {
            if (result.isConfirmed) {
                Swal.fire({
                    title: 'Salvando...',
                    text: 'Por favor, aguarde enquanto o chamado é salvo.',
                    allowOutsideClick: false,  
                    didOpen: () => {
                        Swal.showLoading(); 
                    }
                });

                if (form.checkValidity() === false) {
                    $('#salvarChamado').addClass('was-validated');
                } else {
                    const loadingTimeout = setTimeout(() => {
                        Swal.close();
                    }, 2000);

                    $.ajax({
                        url: '/Chamado/GravarChamado',
                        type: 'POST',
                        data: formData,
                        contentType: false, 
                        processData: false, 
                        success: function (response) {
                            clearTimeout(loadingTimeout);
                            Swal.close();
                            Swal.fire({
                                title: 'Sucesso!',
                                text: 'Chamado salvo com sucesso.',
                                icon: 'success',
                                confirmButtonText: 'OK'
                            }).then(() => {
                                $('#modalCadastroChamado').modal('hide');
                                $('#salvarChamado')[0].reset();
                                $('#salvarChamado').removeClass('was-validated');
                                location.reload();  
                            });
                        },
                        error: function () {
                            clearTimeout(loadingTimeout); 
                            Swal.close();
                            Swal.fire({
                                icon: 'error',
                                title: 'Erro!',
                                text: 'Ocorreu um erro inesperado.',
                                confirmButtonText: 'OK'
                            });
                        }
                    });
                }
            }
        });
    });
});
