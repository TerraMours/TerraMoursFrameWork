namespace TerraMours_Gpt.Domains.GptDomain.Contracts.Res {
    public class ChatRes {
        public ChatRes()
        {
        }

        public ChatRes(string? text)
        {
            Text = text;
        }

        public string? Text { get; set; }
    }
}
