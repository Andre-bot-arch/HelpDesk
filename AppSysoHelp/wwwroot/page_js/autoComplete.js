$(document).ready(function () {
    $('#valor').mask('000.000.000.000.000,00', { reverse: true });

    $('#clienteSelect').select2({
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
                            text: item.label
                        };
                    })
                };
            },

            cache: true
        }
    });

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

});
