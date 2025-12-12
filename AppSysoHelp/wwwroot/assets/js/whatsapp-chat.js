// ============================================
// WHATSAPP CHAT - Multi Conversas
// ============================================

let chatConnection = null;
let currentPhoneNumber = null;
let currentCustomerName = null;
let conversations = [];
let autoRefreshInterval = null;

// ============================================
// INICIALIZAR
// ============================================
async function initializeWhatsAppChat() {
    try {
        console.log('🚀 Inicializando WhatsApp Chat Multi-Conversas.. .');

        // Conectar SignalR
        await connectSignalR();

        // Carregar conversas
        await loadConversations();

        // Configurar busca
        setupSearch();

        // Configurar eventos de envio
        setupSendEvents();

        // Auto-refresh a cada 15 segundos
        autoRefreshInterval = setInterval(loadConversations, 15000);

        console.log('✅ Chat inicializado com sucesso');

    } catch (error) {
        console.error('❌ Erro ao inicializar chat:', error);
        showError('Erro ao inicializar chat.  Recarregue a página.');
    }
}

// ============================================
// SIGNALR
// ============================================
async function connectSignalR() {
    try {
        chatConnection = new signalR.HubConnectionBuilder()
            .withUrl("/chatHub")
            .withAutomaticReconnect()
            .configureLogging(signalR.LogLevel.Information)
            .build();

        // Receber nova mensagem
        chatConnection.on("ReceiveMessage", (message) => {
            console.log('📩 Nova mensagem via SignalR:', message);
            handleNewMessage(message);
        });

        // Reconectando
        chatConnection.onreconnecting(() => {
            console.log('🔄 Reconectando SignalR...');
        });

        // Reconectado
        chatConnection.onreconnected(() => {
            console.log('✅ SignalR reconectado');
            // Reentrar no grupo se houver conversa ativa
            if (currentPhoneNumber) {
                chatConnection.invoke("JoinPhoneGroup", currentPhoneNumber);
            }
        });

        // Desconectado
        chatConnection.onclose(() => {
            console.log('❌ SignalR desconectado');
        });

        await chatConnection.start();
        console.log('✅ SignalR conectado');

    } catch (error) {
        console.error('❌ Erro ao conectar SignalR:', error);
        throw error;
    }
}

// ============================================
// CARREGAR CONVERSAS
// ============================================
async function loadConversations() {
    try {
        const response = await fetch('/api/chat/conversations');

        if (!response.ok) {
            throw new Error(`HTTP ${response.status}`);
        }

        const data = await response.json();
        conversations = data.conversations || [];

        renderConversations(conversations);

        document.getElementById('conversation-count').textContent = conversations.length;

        console.log(`✅ ${conversations.length} conversas carregadas`);

    } catch (error) {
        console.error('❌ Erro ao carregar conversas:', error);
        showConversationsError();
    }
}

// ============================================
// RENDERIZAR CONVERSAS
// ============================================
function renderConversations(conversationList) {
    const container = document.getElementById('conversations-list');

    if (conversationList.length === 0) {
        container.innerHTML = `
            <div class="text-center text-muted py-5">
                <i class="fab fa-whatsapp fa-3x mb-3 text-success"></i>
                <p class="mb-0">Nenhuma conversa ainda</p>
                <small>As conversas aparecerão aqui quando clientes enviarem mensagens</small>
            </div>
        `;
        return;
    }

    container.innerHTML = conversationList.map(conv => {
        const customerName = conv.customerName || conv.phoneNumber;
        const initials = getInitials(customerName);
        const time = formatTime(conv.lastMessageTime);
        const isActive = currentPhoneNumber === conv.phoneNumber;
        const preview = getMessagePreview(conv.lastMessageType, conv.lastMessage);

        return `
            <div class="conversation-item ${isActive ? 'active' : ''}" 
                 data-phone="${escapeHtml(conv.phoneNumber)}"
                 onclick="openConversation('${escapeHtml(conv.phoneNumber)}', '${escapeHtml(customerName)}')">
                <div class="conversation-avatar">${initials}</div>
                <div class="conversation-info">
                    <div class="conversation-header">
                        <div class="conversation-name">${escapeHtml(customerName)}</div>
                        <div class="conversation-time">${time}</div>
                    </div>
                    <div class="conversation-footer">
                        <div class="conversation-last-message">${preview}</div>
                        ${conv.unreadCount > 0 ? `<div class="unread-badge">${conv.unreadCount}</div>` : ''}
                    </div>
                </div>
            </div>
        `;
    }).join('');
}

// ============================================
// ABRIR CONVERSA
// ============================================
async function openConversation(phoneNumber, customerName) {
    try {
        console.log(`📱 Abrindo conversa:  ${customerName} (${phoneNumber})`);

        currentPhoneNumber = phoneNumber;
        currentCustomerName = customerName;

        // Atualizar UI
        document.getElementById('no-chat-selected').classList.add('d-none');
        document.getElementById('active-chat').classList.remove('d-none');
        document.getElementById('active-chat').classList.add('d-flex');

        const initials = getInitials(customerName);
        document.getElementById('chat-avatar-initials').textContent = initials;
        document.getElementById('chat-customer-name').textContent = customerName;
        document.getElementById('chat-phone-number').textContent = phoneNumber;

        // Marcar como ativa na lista
        document.querySelectorAll('.conversation-item').forEach(item => {
            item.classList.remove('active');
        });
        document.querySelector(`.conversation-item[data-phone="${phoneNumber}"]`)?.classList.add('active');

        // ✅ AJUSTAR VIEW MOBILE
        toggleMobileView();

        // Carregar mensagens
        await loadMessages(phoneNumber);

        // Entrar no grupo SignalR
        if (chatConnection) {
            await chatConnection.invoke("JoinPhoneGroup", phoneNumber);
            console.log(`✅ Entrou no grupo do telefone ${phoneNumber}`);
        }

        // Focar input
        document.getElementById('message-input').focus();

    } catch (error) {
        console.error('❌ Erro ao abrir conversa:', error);
        showError('Erro ao abrir conversa');
    }
}

// ============================================
// CARREGAR MENSAGENS
// ============================================
async function loadMessages(phoneNumber) {
    try {
        const response = await fetch(`/api/chat/phone/${phoneNumber}/messages? limit=50`);

        if (!response.ok) {
            throw new Error(`HTTP ${response.status}`);
        }

        const data = await response.json();
        const chatMessages = document.getElementById('chat-messages');
        chatMessages.innerHTML = '';

        if (data.messages.length === 0) {
            chatMessages.innerHTML = `
                <div class="text-center text-muted py-5">
                    <i class="fas fa-comments fa-3x mb-3"></i>
                    <p class="mb-0">Nenhuma mensagem ainda</p>
                    <small>Inicie a conversa enviando uma mensagem</small>
                </div>
            `;
            return;
        }

        data.messages.forEach(msg => {
            appendMessage(msg, false);
        });

        scrollToBottom();

        console.log(`✅ ${data.messages.length} mensagens carregadas`);

    } catch (error) {
        console.error('❌ Erro ao carregar mensagens:', error);
        showError('Erro ao carregar mensagens');
    }
}

// ============================================
// ADICIONAR MENSAGEM
// ============================================
function appendMessage(message, autoScroll = true) {
    const chatMessages = document.getElementById('chat-messages');

    const isIncoming = message.direction === 'incoming';
    const messageClass = isIncoming ? 'message-incoming' : 'message-outgoing';

    const timestamp = new Date(message.timestamp);
    const timeString = timestamp.toLocaleTimeString('pt-BR', {
        hour: '2-digit',
        minute: '2-digit'
    });

    const messageContent = renderMessageContent(message);

    const messageDiv = document.createElement('div');
    messageDiv.className = messageClass;
    messageDiv.setAttribute('data-message-id', message.id);
    messageDiv.innerHTML = `
        <div class="message-bubble">
            ${messageContent}
            <div class="message-info">
                <span>${timeString}</span>
                ${!isIncoming ? '<i class="fas fa-check-double text-primary" title="Enviado"></i>' : ''}
            </div>
        </div>
    `;

    chatMessages.appendChild(messageDiv);

    if (autoScroll) {
        scrollToBottom();
    }
}

// ============================================
// RENDERIZAR CONTEÚDO DA MENSAGEM
// ============================================
function renderMessageContent(message) {
    const messageType = message.messageType || 'text';

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
            return `<p class="message-content">😊 Figurinha</p>`;

        case 'location':
            return `<p class="message-content">📍 ${escapeHtml(message.content)}</p>`;

        case 'contacts':
            return `<p class="message-content">👤 ${escapeHtml(message.content)}</p>`;

        default:
            return `<p class="message-content text-muted"><em>${escapeHtml(message.content)}</em></p>`;
    }
}

function renderImageMessage(message) {
    if (message.mediaUrl) {
        let html = `<div class="message-media">
            <img src="${escapeHtml(message.mediaUrl)}" 
                 alt="Imagem" 
                 class="img-fluid rounded mb-2" 
                 style="max-width: 300px; cursor: pointer;" 
                 onclick="window.open('${escapeHtml(message.mediaUrl)}', '_blank')" />`;

        if (message.content && message.content !== '📷 Imagem') {
            html += `<p class="message-content mt-2">${escapeHtml(message.content)}</p>`;
        }
        html += '</div>';
        return html;
    }
    return `<p class="message-content">📷 ${escapeHtml(message.content)}</p>`;
}

function renderAudioMessage(message) {
    if (message.mediaUrl) {
        return `<div class="message-media">
            <p class="message-content mb-2">🎵 Áudio</p>
            <audio controls class="w-100">
                <source src="${escapeHtml(message.mediaUrl)}" type="audio/ogg">
                <source src="${escapeHtml(message.mediaUrl)}" type="audio/mpeg">
            </audio>
        </div>`;
    }
    return `<p class="message-content">🎵 ${escapeHtml(message.content)}</p>`;
}

function renderVideoMessage(message) {
    if (message.mediaUrl) {
        let html = `<div class="message-media">
            <video controls class="w-100 rounded mb-2" style="max-width: 300px;">
                <source src="${escapeHtml(message.mediaUrl)}" type="video/mp4">
            </video>`;

        if (message.content && message.content !== '🎥 Vídeo') {
            html += `<p class="message-content mt-2">${escapeHtml(message.content)}</p>`;
        }
        html += '</div>';
        return html;
    }
    return `<p class="message-content">🎥 ${escapeHtml(message.content)}</p>`;
}

function renderDocumentMessage(message) {
    let html = `<div class="message-media">
        <i class="fas fa-file-alt fa-2x text-primary mb-2"></i>
        <p class="message-content mb-2">${escapeHtml(message.content)}</p>`;

    if (message.mediaUrl) {
        html += `<a href="${escapeHtml(message.mediaUrl)}" target="_blank" class="btn btn-sm btn-outline-primary">
            <i class="fas fa-download"></i> Baixar
        </a>`;
    }
    html += '</div>';
    return html;
}

// ============================================
// ENVIAR MENSAGEM
// ============================================
async function sendMessage() {
    const input = document.getElementById('message-input');
    const message = input.value.trim();

    if (!message || !currentPhoneNumber) {
        return;
    }

    try {
        console.log(`📤 Enviando mensagem para ${currentPhoneNumber}`);

        input.disabled = true;
        document.getElementById('send-button').disabled = true;

        const response = await fetch('/api/chat/send-to-phone', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                phoneNumber: currentPhoneNumber,
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

        // A mensagem será adicionada via SignalR

    } catch (error) {
        console.error('❌ Erro ao enviar mensagem:', error);
        showError('Erro ao enviar:  ' + error.message);
    } finally {
        input.disabled = false;
        document.getElementById('send-button').disabled = false;
    }
}

// ============================================
// CONFIGURAR EVENTOS DE ENVIO
// ============================================
function setupSendEvents() {
    const sendButton = document.getElementById('send-button');
    const input = document.getElementById('message-input');

    sendButton?.addEventListener('click', sendMessage);

    input?.addEventListener('keypress', (e) => {
        if (e.key === 'Enter' && !e.shiftKey) {
            e.preventDefault();
            sendMessage();
        }
    });
}

// ============================================
// FECHAR CHAT
// ============================================
function closeChat() {
    // Sair do grupo SignalR
    if (chatConnection && currentPhoneNumber) {
        chatConnection.invoke("LeavePhoneGroup", currentPhoneNumber);
    }

    currentPhoneNumber = null;
    currentCustomerName = null;

    document.getElementById('active-chat').classList.add('d-none');
    document.getElementById('active-chat').classList.remove('d-flex');
    document.getElementById('no-chat-selected').classList.remove('d-none');

    document.querySelectorAll('.conversation-item').forEach(item => {
        item.classList.remove('active');
    });

    toggleMobileView();

    console.log('✅ Chat fechado');
}

// ============================================
// DETECTAR REDIMENSIONAMENTO
// ============================================
window.addEventListener('resize', () => {
    toggleMobileView();
});

// ============================================
// BUSCA
// ============================================
function setupSearch() {
    const searchInput = document.getElementById('search-conversations');
    searchInput?.addEventListener('input', (e) => {
        const query = e.target.value.toLowerCase().trim();

        if (!query) {
            renderConversations(conversations);
            return;
        }

        const filtered = conversations.filter(c =>
            c.customerName.toLowerCase().includes(query) ||
            c.phoneNumber.includes(query) ||
            c.lastMessage.toLowerCase().includes(query)
        );

        renderConversations(filtered);
    });
}

// ============================================
// HANDLE NOVA MENSAGEM VIA SIGNALR
// ============================================
function handleNewMessage(message) {
    console.log('🔔 Processando nova mensagem:', message);

    // Se a conversa atual está aberta, adicionar mensagem
    if (message.phoneNumber && message.phoneNumber === currentPhoneNumber) {
        appendMessage(message, true);
    }

    // Atualizar lista de conversas
    loadConversations();
}

// ============================================
// INFORMAÇÕES DA CONVERSA
// ============================================
async function showConversationInfo() {
    if (!currentPhoneNumber) return;

    try {
        const modal = new bootstrap.Modal(document.getElementById('conversationInfoModal'));
        const content = document.getElementById('conversation-info-content');

        modal.show();

        const response = await fetch(`/api/chat/conversation/${currentPhoneNumber}/info`);
        const info = await response.json();

        content.innerHTML = `
            <div class="list-group list-group-flush">
                <div class="list-group-item">
                    <strong>Nome: </strong> ${escapeHtml(info.customerName)}
                </div>
                <div class="list-group-item">
                    <strong>Telefone: </strong> ${escapeHtml(info.phoneNumber)}
                </div>
                <div class="list-group-item">
                    <strong>Chamado ID:</strong> ${info.chamadoId || 'Nenhum'}
                </div>
                <div class="list-group-item">
                    <strong>Total de mensagens:</strong> ${info.messageCount}
                </div>
                <div class="list-group-item">
                    <strong>Estado da sessão:</strong> ${getSessionStateName(info.sessionState)}
                </div>
            </div>
        `;

    } catch (error) {
        console.error('❌ Erro ao buscar informações:', error);
    }
}

// ============================================
// UTILITÁRIOS
// ============================================
function escapeHtml(text) {
    if (!text) return '';
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

function scrollToBottom() {
    const chatMessages = document.getElementById('chat-messages');
    if (chatMessages) {
        chatMessages.scrollTop = chatMessages.scrollHeight;
    }
}

function getInitials(name) {
    if (!name) return '?';
    const words = name.trim().split(' ');
    if (words.length === 1) {
        return words[0].substring(0, 2).toUpperCase();
    }
    return (words[0][0] + words[words.length - 1][0]).toUpperCase();
}

function formatTime(timestamp) {
    const date = new Date(timestamp);
    const now = new Date();
    const diffMs = now - date;
    const diffMins = Math.floor(diffMs / 60000);
    const diffHours = Math.floor(diffMs / 3600000);
    const diffDays = Math.floor(diffMs / 86400000);

    if (diffMins < 1) return 'Agora';
    if (diffMins < 60) return `${diffMins}min`;
    if (diffHours < 24) return date.toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' });
    if (diffDays === 1) return 'Ontem';
    if (diffDays < 7) return date.toLocaleDateString('pt-BR', { weekday: 'short' });
    return date.toLocaleDateString('pt-BR', { day: '2-digit', month: '2-digit' });
}

function getMessagePreview(type, content) {
    switch (type?.toLowerCase()) {
        case 'image': return '📷 Imagem';
        case 'audio': return '🎵 Áudio';
        case 'voice': return '🎤 Áudio';
        case 'video': return '🎥 Vídeo';
        case 'document': return '📄 Documento';
        case 'sticker': return '😊 Figurinha';
        case 'location': return '📍 Localização';
        case 'contacts': return '👤 Contato';
        default: return escapeHtml(content || '').substring(0, 40);
    }
}

function getSessionStateName(state) {
    switch (state) {
        case 0: return 'Ativo (Bot)';
        case 1: return 'Aguardando Atendente';
        case 2: return 'Atendente Ativo';
        default: return 'Desconhecido';
    }
}

function showError(message) {
    // Você pode usar toast/alert do Bootstrap aqui
    alert(message);
}

function showConversationsError() {
    const container = document.getElementById('conversations-list');
    container.innerHTML = `
        <div class="alert alert-danger m-3">
            <i class="fas fa-exclamation-triangle"></i>
            Erro ao carregar conversas.  
            <button class="btn btn-sm btn-outline-danger ms-2" onclick="loadConversations()">
                Tentar novamente
            </button>
        </div>
    `;
}

function toggleMobileView() {
    if (window.innerWidth < 768) {
        const conversationsCol = document.getElementById('conversations-column');
        const chatCol = document.getElementById('chat-column');

        if (currentPhoneNumber) {
            // Chat aberto - esconder conversas
            conversationsCol?.classList.add('hide-mobile');
            chatCol?.classList.add('show-mobile');
        } else {
            // Chat fechado - mostrar conversas
            conversationsCol?.classList.remove('hide-mobile');
            chatCol?.classList.remove('show-mobile');
        }
    }
}

function backToConversations() {
    closeChat();
    toggleMobileView();
}


// ============================================
// INICIALIZAR QUANDO DOM CARREGAR
// ============================================
document.addEventListener('DOMContentLoaded', initializeWhatsAppChat);

// Limpar ao sair
window.addEventListener('beforeunload', () => {
    if (chatConnection && currentPhoneNumber) {
        chatConnection.invoke("LeavePhoneGroup", currentPhoneNumber);
    }
    if (autoRefreshInterval) {
        clearInterval(autoRefreshInterval);
    }
});