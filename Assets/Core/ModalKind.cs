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
        Computer = 8,
        ItemInspect = 9,
        Memories = 10
        // Password, cables, letras, escotilla, escritorio, inspect 3D y memorias de la pausa.
    }
    // Orden numérico = prioridad... El stack real es LIFO
}
