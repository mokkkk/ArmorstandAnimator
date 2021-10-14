namespace ArmorstandAnimator
{
    public interface CurrentFile
    {
        public void Initialize(string[] paths);

        public void SelectPath(string path);

        public void DecidePath();

        public void Cancel();
    }
}