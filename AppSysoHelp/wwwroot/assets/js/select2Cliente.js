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
});