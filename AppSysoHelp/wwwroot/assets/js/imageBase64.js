$(document).ready(function () {
    function readURL(input, previewId, callback) {
        if (input.files && input.files[0]) {
            var reader = new FileReader();
            reader.onload = function (e) {
                $(previewId).attr('src', e.target.result).show();
                if (callback) {
                    callback(e.target.result); // Envia a imagem em Base64 para o callback
                }
            };
            reader.readAsDataURL(input.files[0]);
        }
    }

    // Variáveis para armazenar a imagem Base64 e a extensão do arquivo
    var imagemBase64 = '';
    var extensaoArquivo = '';

    // Função para remover o cabeçalho e extrair a extensão do arquivo
    function processBase64(base64) {
        // Dividir a string base64 no cabeçalho e no conteúdo
        var base64Parts = base64.split(',');
        var mimeType = base64Parts[0].match(/:(.*?);/)[1]; // Extrai o mime type, ex: "image/png"
        var pureBase64 = base64Parts[1]; // Essa é a parte pura em Base64

        // Verificar a extensão com base no mime type
        var extensao = mimeType.split('/')[1]; // Exemplo: "png", "jpeg"

        return {
            base64: pureBase64,
            extensao: extensao
        };
    }

    // Pré-visualizar imagem no modal de Cadastrar
    $("#imagemBase64Cadastrar").change(function () {
        readURL(this, '#previewImagemCadastrar', function (base64) {
            var result = processBase64(base64);
            imagemBase64 = result.base64; // Armazena o Base64 da imagem sem o cabeçalho
            extensaoArquivo = result.extensao;


        });
    });

    // Pré-visualizar imagem no modal de Editar
    $("#imagemBase64Editar").change(function () {
        readURL(this, '#previewImagemEditar', function (base64) {
            var result = processBase64(base64);
            imagemBase64 = result.base64; // Armazena o Base64 da imagem sem o cabeçalho
            extensaoArquivo = result.extensao; // Armazena a extensão do arquivo
        });
    });
});