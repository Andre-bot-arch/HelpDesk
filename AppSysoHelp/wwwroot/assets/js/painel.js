$(function () {
    // Inicializa as Variáveis
    var _senhaChamado = false;
    var _PainelSelecionado = 0;
    var _PainelAtendimento = 1; // 1 Atendimento
    var _AudioDing = document.getElementById('audio-ding');
    var _AudioSenha = document.getElementById('audio-senha');

    // Função de clique para iniciar o painel
    $("#button-iniciar-painel").on("click", function (e) {
        // Definindo painel selecionado
        _PainelSelecionado = _PainelAtendimento;

        // Fechar o modal (se existir)
        $('#modalSelecionarPainel').modal('hide');

        // Garantir que o áudio seja tocado
        tocarAudio(_AudioDing, function () {
            intervalo();
            UltimasChamadas(_PainelSelecionado);
        });
    });

    // Função para tocar o áudio (Ding ou Senha)
    function tocarAudio(audioElement, callback) {
        let promise = audioElement.play();

        if (promise !== undefined) {
            promise.then(() => {
                console.log('Som tocando');
            }).catch((e) => {
                console.error('Erro ao tocar áudio');
                console.error(e);
            });
        }

        audioElement.onended = function () {
            if (callback) callback();
        };
    }

    // Função Verificar a Proxima Senha a Ser Chamada
    function GetSenhaChamar(id_painel) {
        _senhaChamado = true;

        $.ajax({
            url: '/Home/GetSenhaPainel',
            dataType: "json",
            contentType: "application/json; charset=utf-8",
            type: "POST",
            data: JSON.stringify({ id_painel: 1 }),
            success: function (data) {
                if (data.length !== 0) {
                    _senhaChamado = true;

                    _AudioSenha.src = `/Senhas/${data[0].id}/Audio`;

                    // Template para a senha atual chamando
                    var template = Handlebars.compile($("#senha-atual-chamando").html());
                    $('#chamada-atual').html(template(data));

                    // Tocar o som de "ding"
                    tocarAudio(_AudioDing, function () {
                        PlayAudioSenha(data);
                    });
                } else {
                    _senhaChamado = false;
                }
            },
            error: function (error) {
                console.error('Erro ao buscar a senha:', error);
            }
        });
    }

    // Função para Buscar as 3 Últimas Chamadas
    function UltimasChamadas(id_painel) {
        $.ajax({
            url: '/Home/UltimasSenhasChamadas',
            dataType: "json",
            contentType: "application/json; charset=utf-8",
            type: "POST",
            data: JSON.stringify({ id_painel: id_painel }),
            success: function (data) {
                var template = Handlebars.compile($("#template-ultimas-chamadas").html());
                $('#ultimas-senhas-chamadas').html(template(data));
            },
            error: function (error) {
                console.error('Erro ao buscar últimas chamadas:', error);
            }
        });
    }

    // Função para tocar a senha (sintetizar áudio)
    function PlayAudioSenha(dados) {
        var setor = dados[0].nomesetor;
        setor = setor.replace(/nutricao/gi, "NUTRIÇÃO").replace(/servico/gi, "SERVIÇO").replace(/consultorio/gi, "CONSULTORÍO").replace(/ambulatorio/gi, "AMBULATORÍO");

        var text = "Paciente: " + "  . . .   " + dados[0].paciente + " . . . . . " + "Comparecer a:  " + setor;

        var utterance = new SpeechSynthesisUtterance(text);
        utterance.lang = 'pt-BR';
        utterance.voice = speechSynthesis.getVoices().find(function (voice) {
            return voice.name === 'Google português do Brasil';
        });

        utterance.onend = function () {
            _senhaChamado = false;
            UltimasChamadas(_PainelSelecionado);
        };

        speechSynthesis.speak(utterance);

        _senhaChamado = true;
        console.log('Chamando a Senha via Áudio');
    }

    // Chamar a função a cada meio segundo
    function intervalo() {
        setInterval(function () {
            if (_senhaChamado === false) {
                GetSenhaChamar(_PainelSelecionado);
            }
        }, 500);
    }

});
