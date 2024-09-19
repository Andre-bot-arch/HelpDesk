$(document).ready(function () {
    $('#btnAtualizarClientes').on('click', function (e) {
        e.preventDefault(); // Evita comportamento padrão do link
        Swal.fire({
            title: 'Tem certeza?',
            text: "Deseja atualizar os clientes agora?",
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
                    text: 'Por favor, aguarde enquanto os clientes são atualizados.',
                    allowOutsideClick: false,  // Impede fechar o Swal clicando fora dele
                    didOpen: () => {
                        Swal.showLoading(); // Exibe o ícone de carregamento
                    }
                });

                $.ajax({
                    url: '/Clientes/BuscarAtualizarClienteSolution', // Ajuste a rota conforme necessário
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
                                text: 'Ocorreu um erro ao atualizar os clientes.',
                                icon: 'error',
                                confirmButtonText: 'OK'
                            });
                        }
                    },
                    error: function () {
                        Swal.close(); // Fecha o Swal com o loader
                        Swal.fire({
                            title: 'Erro!',
                            text: 'Não foi possível atualizar os clientes.',
                            icon: 'error',
                            confirmButtonText: 'OK'
                        });
                    }
                });
            }
        });
    });
});