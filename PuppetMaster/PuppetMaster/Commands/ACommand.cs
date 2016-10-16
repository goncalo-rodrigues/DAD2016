namespace PuppetMaster
{
    abstract class ACommand
    {
        protected PuppetMaster master { get; set; }
        public string name { get; set; }
        public string help { get; set; }
        

        public ACommand(PuppetMaster master, string name) : this(master, name, "<no help>") { }
        public ACommand(PuppetMaster master, string name, string help)
        {
            this.name = name; this.help = help;
        }
        public abstract void execute(string[] args);

    }
}
