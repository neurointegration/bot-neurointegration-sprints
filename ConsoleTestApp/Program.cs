using BotTemplate;
using Yandex.Cloud.Functions;

var handler = new TelegramHandler();
var request = "{ \"body\": \"123\" }";
Context context = null;

handler.FunctionHandler(request, context);