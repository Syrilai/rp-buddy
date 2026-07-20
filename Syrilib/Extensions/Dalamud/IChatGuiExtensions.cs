// ReSharper disable MemberCanBeMadeStatic.Local
// ReSharper disable MemberCanBeMadeStatic.Global
// ReSharper disable MemberCanBePrivate.Global

using System;
using System.Text;
using Dalamud.Memory;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Shell;

namespace Syrilib.Extensions.Dalamud;

// ReSharper disable once InconsistentNaming
public static class IChatGuiExtensions
{
    extension(IChatGui chatGui)
    {
        public unsafe void ExecuteCommand(string command)
        {
            if (!command.StartsWith('/'))
                return;
            using var utf8Command = new Utf8String(command);
            RaptureShellModule.Instance()->ExecuteCommandInner(&utf8Command, UIModule.Instance());
        }

        private unsafe void SendMessageUnsafe(byte[] byteMessage)
        {
            var utf8Message = Utf8String.FromSequence(byteMessage.NullTerminate());
            UIModule.Instance()->ProcessChatBoxEntry(utf8Message);
            utf8Message->Dtor(true);
        }

        public void SendMessage(string message)
        {
            var bytes = Encoding.UTF8.GetBytes(message);
            switch (bytes.Length)
            {
                case 0:
                    throw new ArgumentException("Message is empty", nameof(message));
                case > 500:
                    throw new ArgumentException("Message is too long", nameof(message));
            }
            if (message.Length != chatGui.SanitiseText(message).Length)
                throw new ArgumentException("Message contains invalid characters", nameof(message));

            chatGui.SendMessageUnsafe(bytes);
        }

        public unsafe string SanitiseText(string text)
        {
            var utf8Text = Utf8String.FromString(text);

            utf8Text->SanitizeString(
                AllowedEntities.UppercaseLetters
                | AllowedEntities.LowercaseLetters
                | AllowedEntities.Numbers
                | AllowedEntities.SpecialCharacters
                | AllowedEntities.CharacterList
                | AllowedEntities.OtherCharacters
                | AllowedEntities.Payloads
                | AllowedEntities.Unknown9
            );

            var sanitisedText = utf8Text->ToString();
            utf8Text->Dtor(true);

            return sanitisedText;
        }
    }
}