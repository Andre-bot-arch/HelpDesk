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
            url: '/Home/BuscarChamadosAberto',
            type: "POST",
            success: function (data) {
                if (data.length !== 0) {
                    _senhaChamado = true;

                   // _AudioSenha.src = `/Senhas/${data[0].id}/Audio`;

                    // Template para a senha atual chamando
                    //var template = Handlebars.compile($("#senha-atual-chamando").html());
                    //$('#chamada-atual').html(template(data));

                    // Tocar o som de "ding"
                    tocarAudio(_AudioDing, function () {
                        PlayAudioSenha(data);
                    });
                } else {
                    _senhaChamado = false;
                }
            },
            error: function (error) {
                console.log('Erro ao buscar a senha:', error);
            }
        });
    }

    // Função para Buscar as 3 Últimas Chamadas
    function UltimasChamadas(id_painel) {
        $.ajax({
            url: '/Home/BuscarChamadosPainel',
            type: "POST",
            success: function (data) {
                $('.corpoChamados').html(data);
            },
            error: function (error) {
                console.log('Erro ao buscar últimas chamadas:', error);
            }
        });
    }

    // Função para tocar a senha (sintetizar áudio)
    function PlayAudioSenha(dados) {
        if (speechSynthesis.speaking) {
            console.log('Já está falando, aguardando...');
            return;
        }

        var cliente = dados.cliente;
        var data = dados.data;
        var prioridade = dados.prioridade;
        var tecnico = (dados.tecnico.length > 1) ? `T\u00e9cnico: ${dados.tecnico}.` : "";
      
        var text = `Aten\u00e7\u00e3o! ${tecnico} Chamado com a prioridade: ${prioridade}. Para o cliente: ${cliente}. Aberto \u00e0s: ${data}.`;

        var utterance = new SpeechSynthesisUtterance(text);
        utterance.lang = 'pt-BR';
        utterance.rate = 0.9; // Velocidade natural
        utterance.pitch = 1.0; // Tom neutro

        
        speechSynthesis.onvoiceschanged = function () {
            var voices = speechSynthesis.getVoices();
            var voice = voices.find(v => v.name.includes("Google português do Brasil"));
            if (voice) {
                utterance.voice = voice;
            }
            speechSynthesis.speak(utterance);
        };

        utterance.onend = function () {
            _senhaChamado = false;
            UltimasChamadas(_PainelSelecionado);
            console.log('Vocalização concluída.');
        };

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
