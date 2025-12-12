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
            console.error('❌ CHAT_CONFIG não encontrado! ');
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
            console.log('🔄 Reconectando SignalR.. .');
            updateConnectionStatus('connecting', 'Reconectando.. .');
        });

        // Evento: Reconectado
        chatConnection.onreconnected(() => {
            console.log('✅ SignalR reconectado! ');
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
        updateConnectionStatus('connecting', 'Conectando.. .');
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
        senderBadge = '<span class="sender-badge bg-warning text-dark">👤 Cliente</span>';
    } else if (message.sentBy === 'agent') {
        senderBadge = '<span class="sender-badge bg-success text-white">👨‍💻 Você</span>';
    }

    // ✅ RENDERIZAR CONTEÚDO BASEADO NO TIPO DE MENSAGEM
    const messageContent = renderMessageContent(message);

    // Criar elemento da mensagem
    const messageDiv = document.createElement('div');
    messageDiv.className = messageClass;
    messageDiv.innerHTML = `
        <div class="message-bubble">
            ${messageContent}
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
// RENDERIZAR CONTEÚDO DA MENSAGEM POR TIPO
// ============================================
function renderMessageContent(message) {
    const messageType = message.messageType || 'text';

    console.log(`🎨 Renderizando mensagem tipo: ${messageType}`, message);

    switch (messageType.toLowerCase()) {
        case 'text':
        case 'interactive':
        case 'button':
            return `<p class="message-content">${escapeHtml(message.content)}</p>`;

        case 'image':
            return renderImageMessage(message);

        case 'audio':
        case 'voice':
            return renderAudioMessage(message);

        case 'video':
            return renderVideoMessage(message);

        case 'document':
            return renderDocumentMessage(message);

        case 'sticker':
            return renderStickerMessage(message);

        case 'location':
            return renderLocationMessage(message);

        case 'contacts':
            return renderContactMessage(message);

        default:
            return `<p class="message-content text-muted"><em>${escapeHtml(message.content)}</em></p>`;
    }
}

// ============================================
// RENDERIZAR TIPOS ESPECÍFICOS DE MENSAGEM
// ============================================

function renderImageMessage(message) {
    let html = '<div class="message-media">';

    if (message.mediaUrl) {
        html += `
            <img src="${escapeHtml(message.mediaUrl)}" 
                 alt="Imagem" 
                 class="img-fluid rounded mb-2" 
                 style="max-width: 300px; cursor: pointer;" 
                 onclick="window.open('${escapeHtml(message.mediaUrl)}', '_blank')" />
        `;
    } else if (message.mediaId) {
        html += `
            <div class="media-placeholder bg-light p-3 rounded mb-2">
                <i class="fas fa-image fa-2x text-muted"></i>
                <p class="mb-0 mt-2"><small>📷 Imagem (ID: ${escapeHtml(message.mediaId)})</small></p>
                <button class="btn btn-sm btn-outline-primary mt-2" onclick="downloadMedia('${escapeHtml(message.mediaId)}', 'image')">
                    <i class="fas fa-download"></i> Baixar
                </button>
            </div>
        `;
    } else {
        html += `<p class="message-content">📷 ${escapeHtml(message.content)}</p>`;
    }

    // Adicionar caption se houver
    if (message.content && message.content !== '📷 Imagem') {
        html += `<p class="message-content mt-2">${escapeHtml(message.content)}</p>`;
    }

    html += '</div>';
    return html;
}

function renderAudioMessage(message) {
    let html = '<div class="message-media">';
    html += '<i class="fas fa-volume-up me-2"></i>';

    if (message.mediaUrl) {
        html += `
            <p class="message-content mb-2">${escapeHtml(message.content)}</p>
            <audio controls class="w-100" style="max-width: 300px;">
                <source src="${escapeHtml(message.mediaUrl)}" type="audio/ogg">
                <source src="${escapeHtml(message.mediaUrl)}" type="audio/mpeg">
                Seu navegador não suporta áudio.
            </audio>
        `;
    } else if (message.mediaId) {
        html += `
            <p class="message-content">${escapeHtml(message.content)}</p>
            <button class="btn btn-sm btn-outline-primary mt-2" onclick="downloadMedia('${escapeHtml(message.mediaId)}', 'audio')">
                <i class="fas fa-download"></i> Baixar áudio
            </button>
        `;
    } else {
        html += `<p class="message-content">${escapeHtml(message.content)}</p>`;
    }

    html += '</div>';
    return html;
}

function renderVideoMessage(message) {
    let html = '<div class="message-media">';

    if (message.mediaUrl) {
        html += `
            <video controls class="w-100 rounded mb-2" style="max-width: 300px;">
                <source src="${escapeHtml(message.mediaUrl)}" type="video/mp4">
                Seu navegador não suporta vídeo. 
            </video>
        `;
    } else if (message.mediaId) {
        html += `
            <div class="media-placeholder bg-light p-3 rounded mb-2">
                <i class="fas fa-video fa-2x text-muted"></i>
                <p class="mb-0 mt-2"><small>🎥 Vídeo</small></p>
                <button class="btn btn-sm btn-outline-primary mt-2" onclick="downloadMedia('${escapeHtml(message.mediaId)}', 'video')">
                    <i class="fas fa-download"></i> Baixar
                </button>
            </div>
        `;
    } else {
        html += `<p class="message-content">🎥 ${escapeHtml(message.content)}</p>`;
    }

    // Adicionar caption se houver
    if (message.content && message.content !== '🎥 Vídeo') {
        html += `<p class="message-content mt-2">${escapeHtml(message.content)}</p>`;
    }

    html += '</div>';
    return html;
}

function renderDocumentMessage(message) {
    return `
        <div class="message-media">
            <i class="fas fa-file-alt fa-2x text-primary"></i>
            <p class="message-content mt-2 mb-2">${escapeHtml(message.content)}</p>
            ${message.mediaUrl ? `
                <a href="${escapeHtml(message.mediaUrl)}" target="_blank" class="btn btn-sm btn-outline-primary">
                    <i class="fas fa-download"></i> Baixar documento
                </a>
            ` : message.mediaId ? `
                <button class="btn btn-sm btn-outline-primary" onclick="downloadMedia('${escapeHtml(message.mediaId)}', 'document')">
                    <i class="fas fa-download"></i> Baixar documento
                </button>
            ` : ''}
        </div>
    `;
}

function renderStickerMessage(message) {
    return `
        <div class="message-media">
            <i class="far fa-smile fa-3x text-warning"></i>
            <p class="message-content mt-2">${escapeHtml(message.content)}</p>
        </div>
    `;
}

function renderLocationMessage(message) {
    return `
        <div class="message-media">
            <i class="fas fa-map-marker-alt fa-2x text-danger"></i>
            <p class="message-content mt-2">${escapeHtml(message.content)}</p>
        </div>
    `;
}

function renderContactMessage(message) {
    return `
        <div class="message-media">
            <i class="fas fa-address-card fa-2x text-info"></i>
            <p class="message-content mt-2">${escapeHtml(message.content)}</p>
        </div>
    `;
}

// ============================================
// DOWNLOAD DE MÍDIA
// ============================================
async function downloadMedia(mediaId, mediaType) {
    try {
        console.log(`📥 Baixando mídia: ${mediaId} (${mediaType})`);

        // Você precisará implementar um endpoint para download
        const response = await fetch(`/api/chat/media/${mediaId}`);

        if (!response.ok) {
            throw new Error('Erro ao baixar mídia');
        }

        const blob = await response.blob();
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `${mediaType}_${mediaId}`;
        document.body.appendChild(a);
        a.click();
        window.URL.revokeObjectURL(url);
        document.body.removeChild(a);

        console.log('✅ Mídia baixada com sucesso');
    } catch (error) {
        console.error('❌ Erro ao baixar mídia:', error);
        alert('Erro ao baixar arquivo');
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
        console.log(`📤 Enviando mensagem: "${message}"`);

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
    if (!text) return '';
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