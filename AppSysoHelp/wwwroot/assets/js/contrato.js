$(document).ready(function () {
    $('#btnAtualizarContratos').on('click', function (e) {
        e.preventDefault(); // Evita comportamento padrão do link
        Swal.fire({
            title: 'Tem certeza?',
            text: "Deseja atualizar os Contratos agora?",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: 'Sim, atualizar!',
            cancelButtonText: 'Cancelar'
        }).then((result) => {
            if (result.isConfirmed) {
                // Exibe o loader antes de iniciar a requisição
                Swal.fire({
                    title: 'Atualizando...',
                    text: 'Por favor, aguarde enquanto os contratos são atualizados.',
                    allowOutsideClick: false,  // Impede fechar o Swal clicando fora dele
                    didOpen: () => {
                        Swal.showLoading(); // Exibe o ícone de carregamento
                    }
                });

                $.ajax({
                    url: '/Contrato/BuscarAtualizarContratoSolution', // Ajuste a rota conforme necessário
                    type: 'POST', // Ou 'GET', dependendo de como sua rota é configurada
                    success: function (data) {
                        // Fecha o loader e exibe a mensagem de sucesso ou erro
                        Swal.close(); // Fecha o Swal com o loader
                        if (data.success) {
                            Swal.fire({
                                title: 'Atualização Completa!',
                                text: data.message,
                                icon: 'success',
                                confirmButtonText: 'OK'
                            });
                        } else {
                            Swal.fire({
                                title: 'Erro!',
                                text: 'Ocorreu um erro ao atualizar os contratos.',
                                icon: 'error',
                                confirmButtonText: 'OK'
                            });
                        }
                    },
                    error: function () {
                        Swal.close(); // Fecha o Swal com o loader
                        Swal.fire({
                            title: 'Erro!',
                            text: 'Não foi possível atualizar os contratos.',
                            icon: 'error',
                            confirmButtonText: 'OK'
                        });
                    }
                });
            }
        });
    });


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

});