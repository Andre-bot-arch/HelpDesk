$(document).ready(function () {

    // Validação em tempo real para o campo "Nome"
    $('#name').on('input', function () {
        var name = $(this).val().trim();
        if (name === "") {
            $(this).addClass('is-invalid');
        } else {
            $(this).removeClass('is-invalid');
        }
    });

    // Validação em tempo real para o campo "Email"
    $('#email').on('input', function () {
        var email = $(this).val().trim();
        var emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/; // Expressão regular para validar o email
        if (email === "" || !emailPattern.test(email)) {
            $(this).addClass('is-invalid');
        } else {
            $(this).removeClass('is-invalid');
        }
    });

    // Validação em tempo real para o campo "Telefone"
    $('#phone').on('input', function () {
        var phone = $(this).val().trim();
        if (phone === "" || phone.length !== 15) { // Considerando a máscara (XX) XXXXX-XXXX
            $(this).addClass('is-invalid');
        } else {
            $(this).removeClass('is-invalid');
        }
    });

    // Validação em tempo real para o campo "Mensagem"
    $('#message').on('input', function () {
        var message = $(this).val().trim();
        if (message === "") {
            $(this).addClass('is-invalid');
        } else {
            $(this).removeClass('is-invalid');
        }
    });

    // Remove a classe de erro ao focar no campo
    $('input, textarea').on('focus', function () {
        $(this).removeClass('is-invalid');
    });
    $('#btnEnviarForm').on('click', function (e) {
        e.preventDefault(); // Evita comportamento padrão do link
        Swal.fire({
            title: 'Tem certeza?',
            text: "Deseja enviar o formulário de contato?",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: 'Sim, enviar!',
            cancelButtonText: 'Cancelar'
        }).then((result) => {
            if (result.isConfirmed) {
                // Exibe o loader antes de iniciar a requisição
                Swal.fire({
                    title: 'Enviando...',
                    text: 'Por favor, aguarde enquanto o email é enviado.',
                    allowOutsideClick: false,  // Impede fechar o Swal clicando fora dele
                    didOpen: () => {
                        Swal.showLoading(); // Exibe o ícone de carregamento
                    }
                });

                var name = $('#name').val();
                var email = $('#email').val();
                var phone = $('#phone').val();
                var message = $('#message').val();
                
                $.ajax({
                    url: '/Inicio/SendContactForm', // Ajuste a rota conforme necessário
                    type: 'POST',
                    data: { name: name, email: email, phone: phone, message: message },
                    success: function (data) {
                        // Fecha o loader e exibe a mensagem de sucesso ou erro
                        Swal.close(); // Fecha o Swal com o loader
                        if (data.success) {
                            Swal.fire({
                                title: 'Email de contato enviado com sucesso!',
                                icon: 'success',
                                confirmButtonText: 'OK'
                            });
                        } else {
                            Swal.fire({
                                title: 'Erro!',
                                text: 'Ocorreu um erro ao enviar o email de contato!',
                                icon: 'error',
                                confirmButtonText: 'OK'
                            });
                        }
                    },
                    error: function () {
                        Swal.close(); // Fecha o Swal com o loader
                        Swal.fire({
                            title: 'Erro!',
                            text: 'Ocorreu um erro ao enviar o email de contato!',
                            icon: 'error',
                            confirmButtonText: 'OK'
                        });
                    }
                });
            }
        });
    });
});