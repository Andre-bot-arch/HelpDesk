$(document).ready(function () {

    $(".btn-inativar-contrato").click(function () {
        var id = $(this).data("id");
        var situacao = $(this).data("ativo")
        var men = $(this).data("men");

        Swal.fire({
            icon: 'error',
            title: 'Aten&ccedil;&atilde;o!!',
            text: men+"?",
            confirmButtonText: 'SIM'
        }).then((result) => {
            if (result.isConfirmed) {
                $.ajax({
                    url: '/Contrato/UpdateEstatusContrato',
                    type: 'POST',
                    data: { id: id, ativo: situacao },
                    success: function (response) {
                        if (response.success) {
                            Swal.fire({
                                icon: 'success',
                                title: 'Sucesso!',
                                text: response.message,
                                confirmButtonText: 'OK'
                            }).then((result) => {
                                if (result.isConfirmed) { 
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

    $(".btn-editar-cadastro").click(function () {
        var inicio = $(this).data("inicio");
        var partes = inicio.split('/');
        var dia = partes[0];
        var mes = partes[1];
        var ano = partes[2];
        var anoMesInicio = ano + '-' + mes;

        var fim = $(this).data("fim");
        var partes = fim.split('/');
        var dia = partes[0];
        var mes = partes[1];
        var ano = partes[2];
        var anoMesFim = ano + '-' + mes;

        $("#ContratoId").val($(this).data("id"));
        $("#DescricaoContrato").val($(this).data("desc"));
        $("#Valor").val($(this).data("valor"));
        $("#DataInicio").val(anoMesInicio);
        $("#DataFim").val(anoMesFim);
        $("#PontosContratados").val($(this).data("pontos"));


        var sistema = $(this).data("plataforma");
        var sistemaid = $(this).data("plataformaid");
        var $select = $("#sistemaSelect");
        if ($select.find(`option[value="${sistemaid}"]`).length === 0) {
            $select.append(new Option(sistema, sistemaid));
        }
        $select.val(sistemaid);

        var cliente = $(this).data("cliente");
        var clienteid = $(this).data("clienteid");
        var $select = $("#clienteSelect");
        if ($select.find(`option[value="${clienteid}"]`).length === 0) {
            $select.append(new Option(cliente, clienteid));
        }
        $select.val(clienteid);



        $('#exampleModalCenteredScrollable').modal('show');

    });

    //$('.btn-salvar').click(function () {
    //    var form = $('#salvarLicenca');
    //    var id = $('#ContratoId').val();
    //    $("#idDoContrato").val(id);

    //    if (form[0].checkValidity() === false) {
    //        form.addClass('was-validated');
    //    } else {         
    //        $.ajax({
    //            url: '/Licenca/GravarLicenca',
    //            type: 'POST',
    //            data: form.serialize(),
    //            success: function (response) {
    //                if (response.success) {
    //                    Swal.fire({
    //                        icon: 'success',
    //                        title: 'Sucesso!',
    //                        text: response.message,
    //                        confirmButtonText: 'OK'
    //                    }).then((result) => {
    //                        if (result.isConfirmed) {
    //                            $('#cadastroDeLicenca').modal('hide');
    //                            form[0].reset();
    //                            form.removeClass('was-validated');
    //                            location.reload();
    //                        }
    //                    });
    //                } else {
    //                    Swal.fire({
    //                        icon: 'error',
    //                        title: 'Erro!',
    //                        text: response.message,
    //                        confirmButtonText: 'OK'
    //                    });
    //                }
    //            },
    //            error: function () {
    //                Swal.fire({
    //                    icon: 'error',
    //                    title: 'Erro!',
    //                    text: 'Ocorreu um erro inesperado.',
    //                    confirmButtonText: 'OK'
    //                });
    //            }
    //        });
    //    }
    //});

});