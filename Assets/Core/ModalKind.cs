namespace Enigma.Core
{
    public enum ModalKind
    {
        Zoom = 0,
        Inventory = 1,
        Document = 2,
        Pause = 3,
        CodeEntry = 4,
        WirePuzzle = 5,
        WordEntry = 6,
        ArrowLock = 7,
        Computer = 8
        // Password, cables, letras, escotilla y escritorio de PC.
    }
    // Orden numérico = prioridad... El stack real es LIFO
}
