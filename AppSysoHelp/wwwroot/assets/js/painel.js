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
            intervalo(); // Iniciar o intervalo para chamadas contínuas
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
                if (data !== null) {
                    // Tocar o som de "ding" e continuar o processo
                    tocarAudio(_AudioDing, function () {
                        PlayAudioSenha(data);
                    });
                } else {
                    UltimasChamadas(_PainelSelecionado);
                    _senhaChamado = false; // Se não houver dados, permitir nova chamada
                }
            },
            error: function (error) {
                console.log('Erro ao buscar a senha:', error);
                _senhaChamado = false; // Permitir nova tentativa em caso de erro
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
            _senhaChamado = false; // Permitir nova chamada após falar
            UltimasChamadas(_PainelSelecionado);
            console.log('Vocalização concluída.');
        };
        updateChamado(dados.id);
        console.log('Chamando a Senha via Áudio');
    }

    function updateChamado(idChamado) {
        $.ajax({
            url: '/Home/UpdateChamadosAberto',
            type: "POST",
            data: { id: idChamado },
            success: function (data) {
                $('.corpoChamados').html(data);
                _senhaChamado = false; // Permitir nova chamada após falar
            },
            error: function (error) {
                console.log('Erro ao atualizar chamados:', error);
                _senhaChamado = false; // Permitir nova chamada após falar
            }
        });
    }

    // Chamar a função a cada meio segundo
    function intervalo() {
        setInterval(function () {
            if (_senhaChamado === false) {
                GetSenhaChamar(_PainelSelecionado);
            }
        }, 60000); // Chamadas contínuas a cada 500ms
    }
});
