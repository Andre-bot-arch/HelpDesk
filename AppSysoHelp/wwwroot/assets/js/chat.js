// ============================================
// CHAT WHATSAPP - SignalR + API REST
// ============================================

let chatConnection = null;
let chatChamadoId = null;

// ============================================
// INICIALIZAR CHAT
// ============================================
async function initializeChat() {
    try {
        // Pegar configurações do HTML
        if (!window.CHAT_CONFIG) {
            console.error('❌ CHAT_CONFIG não encontrado!');
            return;
        }

        chatChamadoId = window.CHAT_CONFIG.chamadoId;

        console.log(`🚀 Inicializando chat para chamado #${chatChamadoId}`);

        // Conectar SignalR
        await connectSignalR();

        // Carregar histórico de mensagens
        await loadMessageHistory();

        // Configurar eventos de envio
        setupSendEvents();

    } catch (error) {
        console.error('❌ Erro ao inicializar chat:', error);
        updateConnectionStatus('disconnected', 'Erro ao conectar');
    }
}

// ============================================
// CONECTAR SIGNALR
// ============================================
async function connectSignalR() {
    try {
        // Criar conexão SignalR
        chatConnection = new signalR.HubConnectionBuilder()
            .withUrl("/chatHub")
            .withAutomaticReconnect()
            .configureLogging(signalR.LogLevel.Information)
            .build();

        // Evento:  Receber mensagem em tempo real
        chatConnection.on("ReceiveMessage", (message) => {
            console.log('📩 Nova mensagem recebida via SignalR:', message);
            appendMessage(message);
        });

        // Evento: Reconectando
        chatConnection.onreconnecting(() => {
            console.log('🔄 Reconectando SignalR...');
            updateConnectionStatus('connecting', 'Reconectando...');
        });

        // Evento: Reconectado
        chatConnection.onreconnected(() => {
            console.log('✅ SignalR reconectado!');
            updateConnectionStatus('connected', 'Conectado');
            // Entrar no grupo novamente
            chatConnection.invoke("JoinChamadoGroup", chatChamadoId.toString());
        });

        // Evento: Desconectado
        chatConnection.onclose(() => {
            console.log('❌ SignalR desconectado');
            updateConnectionStatus('disconnected', 'Desconectado');
        });

        // Conectar
        updateConnectionStatus('connecting', 'Conectando...');
        await chatConnection.start();
        console.log('✅ SignalR conectado! ');
        updateConnectionStatus('connected', 'Conectado');

        // Entrar no grupo do chamado
        await chatConnection.invoke("JoinChamadoGroup", chatChamadoId.toString());
        console.log(`✅ Entrou no grupo do chamado #${chatChamadoId}`);

    } catch (error) {
        console.error('❌ Erro ao conectar SignalR:', error);
        updateConnectionStatus('disconnected', 'Erro');
        throw error;
    }
}

// ============================================
// CARREGAR HISTÓRICO DE MENSAGENS
// ============================================
async function loadMessageHistory() {
    try {
        console.log(`📥 Carregando histórico do chamado #${chatChamadoId}...`);

        const response = await fetch(`/api/chat/${chatChamadoId}/messages`);

        if (!response.ok) {
            throw new Error(`HTTP ${response.status}`);
        }

        const data = await response.json();
        const messages = data.messages || [];

        console.log(`✅ ${messages.length} mensagens carregadas`);

        // Limpar área de mensagens
        const chatMessages = document.getElementById('chat-messages');
        chatMessages.innerHTML = '';

        // Se não houver mensagens
        if (messages.length === 0) {
            chatMessages.innerHTML = `
                <div class="text-center text-muted py-5">
                    <i class="fab fa-whatsapp fa-3x mb-3"></i>
                    <p>Nenhuma mensagem ainda</p>
                    <small>As mensagens aparecerão aqui em tempo real</small>
                </div>
            `;
            return;
        }

        // Renderizar mensagens (mais antigas primeiro)
        messages.reverse().forEach(msg => {
            appendMessage(msg, false); // false = não fazer scroll
        });

        // Scroll para última mensagem
        scrollToBottom();

    } catch (error) {
        console.error('❌ Erro ao carregar histórico:', error);
        document.getElementById('chat-messages').innerHTML = `
            <div class="alert alert-danger m-3">
                Erro ao carregar mensagens.  Tente recarregar a página.
            </div>
        `;
    }
}

// ============================================
// ADICIONAR MENSAGEM NA UI
// ============================================
function appendMessage(message, autoScroll = true) {
    const chatMessages = document.getElementById('chat-messages');

    // Remover mensagem de "carregando" se existir
    const loadingMsg = chatMessages.querySelector('.text-center');
    if (loadingMsg) {
        loadingMsg.remove();
    }

    const isIncoming = message.direction === 'incoming';
    const messageClass = isIncoming ? 'message-incoming' : 'message-outgoing';

    // Formatar timestamp
    const timestamp = message.timestamp ? new Date(message.timestamp) : new Date();
    const timeString = timestamp.toLocaleTimeString('pt-BR', {
        hour: '2-digit',
        minute: '2-digit'
    });

    // Badge do remetente
    let senderBadge = '';
    if (message.sentBy === 'bot') {
        senderBadge = '<span class="sender-badge bg-info text-white">🤖 Bot</span>';
    } else if (message.sentBy === 'customer') {
        senderBadge = '<span class="sender-badge bg-secondary text-white">👤 Cliente</span>';
    } else if (message.sentBy === 'agent') {
        senderBadge = '<span class="sender-badge bg-success text-white">👨‍💻 Você</span>';
    }

    // Criar elemento da mensagem
    const messageDiv = document.createElement('div');
    messageDiv.className = messageClass;
    messageDiv.innerHTML = `
        <div class="message-bubble">
            <p class="message-content">${escapeHtml(message.content)}</p>
            <div class="message-info">
                <span>${timeString}</span>
                ${senderBadge}
            </div>
        </div>
    `;

    chatMessages.appendChild(messageDiv);

    // Scroll automático
    if (autoScroll) {
        scrollToBottom();
    }
}

// ============================================
// ENVIAR MENSAGEM
// ============================================
async function sendMessage() {
    const input = document.getElementById('message-input');
    const message = input.value.trim();

    if (!message) {
        return;
    }

    try {
        console.log(`📤 Enviando mensagem:  "${message}"`);

        // Desabilitar input temporariamente
        input.disabled = true;
        document.getElementById('send-button').disabled = true;

        const response = await fetch('/api/chat/send', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                chamadoId: chatChamadoId,
                message: message
            })
        });

        if (!response.ok) {
            const error = await response.json();
            throw new Error(error.error || 'Erro ao enviar mensagem');
        }

        console.log('✅ Mensagem enviada com sucesso');

        // Limpar input
        input.value = '';
        input.focus();

    } catch (error) {
        console.error('❌ Erro ao enviar mensagem:', error);
        alert('Erro ao enviar mensagem:  ' + error.message);
    } finally {
        // Reabilitar input
        input.disabled = false;
        document.getElementById('send-button').disabled = false;
    }
}

// ============================================
// CONFIGURAR EVENTOS DE ENVIO
// ============================================
function setupSendEvents() {
    const input = document.getElementById('message-input');
    const sendButton = document.getElementById('send-button');

    // Clique no botão
    sendButton.addEventListener('click', sendMessage);

    // Enter no input
    input.addEventListener('keypress', (e) => {
        if (e.key === 'Enter' && !e.shiftKey) {
            e.preventDefault();
            sendMessage();
        }
    });
}

// ============================================
// UTILITÁRIOS
// ============================================

// Atualizar status de conexão
function updateConnectionStatus(status, text) {
    const badge = document.getElementById('connection-status');
    if (!badge) return;

    badge.className = 'badge ms-2 ' + status;
    badge.textContent = text;
}

// Scroll para última mensagem
function scrollToBottom() {
    const chatMessages = document.getElementById('chat-messages');
    chatMessages.scrollTop = chatMessages.scrollHeight;
}

// Escapar HTML (prevenir XSS)
function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

// ============================================
// INICIALIZAR QUANDO DOM CARREGAR
// ============================================
document.addEventListener('DOMContentLoaded', () => {
    // Verificar se está na página de atendimento
    if (document.getElementById('chat-container')) {
        initializeChat();
    }
});

// Sair do grupo ao fechar página
window.addEventListener('beforeunload', () => {
    if (chatConnection && chatChamadoId) {
        chatConnection.invoke("LeaveChamadoGroup", chatChamadoId.toString());
    }
});