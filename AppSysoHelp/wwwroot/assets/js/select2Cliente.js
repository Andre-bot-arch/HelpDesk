$(document).ready(function () {
    // Mostrar/Esconder caixas
    $(".mostrarcaixa").click(function () {
        var definido = $(this).data("id");
        if ($(this).is(":checked")) {
            $(definido).show();
        } else {
            $(definido).hide();
        }
    });

    // Inicializar Select2 com opções AJAX
    $('#clienteSelect88').select2({
        placeholder: 'Buscar Cliente',
        minimumInputLength: 4,
        ajax: {
            url: '/Contrato/GetSugestaoCliente',
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            delay: 250,
            data: function (params) {
                return {
                    query: params.term
                };
            },
            processResults: function (data) {
                return {
                    results: data.map(function (item) {
                        return {
                            id: item.value,
                            text: item.label,
                            endereco: item.endereco,
                            telefone: item.telefone, // Verifique se "telefone" existe na resposta do servidor
                            cnpj: item.cnpj,
                            razaoSocial: item.razaoSocial,
                        };
                    })
                };
            },
            cache: true
        },
        templateResult: formatClienteResult, // Customização das opções
        templateSelection: formatClienteSelection // Customização da opção selecionada
    });

    // Evento ao selecionar um cliente
    $('#clienteSelect88').on('select2:select', function (e) {
        var telefoneCliente = e.params.data.telefone; // Acesse diretamente "telefone" do item selecionado
        $("#TelefoneContato1").val(telefoneCliente);
    });

    // Formatação das opções na lista
    function formatClienteResult(cliente) {
        if (!cliente.id) {
            return cliente.text;
        }

        var $container = $(
                `<div>
                    <strong>${cliente.text}</strong><br/>
                    <small>Endereço: ${cliente.endereco}</small><br/>
                    <small>Telefone: ${cliente.telefone}</small><br/>
                    <small>Razao Social: ${cliente.razaoSocial}</small>
                </div>`
        );

        return $container;
    }

    // Formatação da opção selecionada
    function formatClienteSelection(cliente) {
        return cliente.text || cliente.id;
    }

    $('#sistemaSelect').select2({
        placeholder: 'Buscar Sistema',
        minimumInputLength: 4,
        ajax: {
            url: '/Contrato/GetSugestaoSistema',
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            delay: 250,
            data: function (params) {
                return {
                    query: params.term
                };
            },
            processResults: function (data) {
                return {
                    results: data.map(function (item) {
                        return {
                            id: item.value,
                            text: item.label
                        };
                    })
                };
            },
            cache: true
        }
    });

    $('#tecnicoSelect10').select2({
        placeholder: 'Buscar Técnico',
        minimumInputLength: 4,
        ajax: {
            url: '/Contrato/GetSugestaoTecnico',
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            delay: 250,
            data: function (params) {
                return {
                    query: params.term
                };
            },
            processResults: function (data) {
                return {
                    results: data.map(function (item) {
                        return {
                            id: item.value,
                            text: item.label
                        };
                    })
                };
            },
            cache: true
        }
    });

    $("#fkCategoria").on("change", function () {
        var definido = $(this).val();
        $.ajax({
            url: '/SubCategoria/ListarPorIdCategoria',
            type: 'POST',
            data: { id: definido },
            success: function (response) {
                $("#fkSubCategoria").empty(); 
                var option = $("<option></option>")
                    .val("")
                    .text("Selecione uma SubCategoria");
                $("#fkSubCategoria").append(option);
                if (response.length > 0) {
                    for (var i = 0; i < response.length; i++) {                        
                        var option = $("<option></option>")
                            .val(response[i].prioridade)
                            .text(response[i].descricao);                        
                        $("#fkSubCategoria").append(option);
                    }
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
    });

    $("#fkSubCategoria").on("change", function () {
        var definido = $(this).val();
        $("#_Prioridade").val(definido);
        if (definido == "Urgente") {
            $("#priori").html('<button type="button" style="font-size: 18px; font-weight:bold" disabled class="btn dark-icon btn-dark btn-block">Urgente</button>');
        }
        else if (definido == "Alta") {
            $("#priori").html('<button type="button" style="font-size: 18px; font-weight:bold" disabled class="btn dark-icon btn-danger btn-block">Alta</button>');
        }
        else if (definido == "Media") {
            $("#priori").html('<button type="button" style="font-size: 18px; font-weight:bold" disabled class="btn dark-icon btn-warning btn-block">Media</button>');
        }
        else {
            $("#priori").html('<button type="button" style="font-size: 18px; font-weight:bold; border: 1px solid black" disabled class="btn dark-icon btn btn-block text-black">Normal</button>');
        }
    });

});