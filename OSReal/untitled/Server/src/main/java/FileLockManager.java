import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Path;
import java.util.ArrayList;
import java.util.List;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.Semaphore;
import java.util.stream.Stream;

/**
 * Shared registry of exclusive locks for files in the server filesystem.
 * Callers should acquire a file lock before modifying or removing that file
 * and release it in a finally block.
 */
public final class FileLockManager {
    private final ConcurrentHashMap<Path, Semaphore> fileLocks = new ConcurrentHashMap<>();

    private FileLockManager() {
    }

    private static class InstanceHolder {
        private static final FileLockManager INSTANCE = new FileLockManager();
    }

    public static FileLockManager getInstance() {
        return InstanceHolder.INSTANCE;
    }

    /**
     * Returns the shared one-permit semaphore for an absolute, normalized file path,
     * creating it atomically if this is the first request for that path.
     * CODEX: ConcurrentHashMap.computeIfAbsent prevents concurrent clients from
     * creating separate semaphores for the same normalized path.
     */
    public Semaphore addLock(Path filePath) {
        Path absolutePath = normalize(filePath);
        return fileLocks.computeIfAbsent(absolutePath, ignored -> new Semaphore(1, true));
    }

    /** Returns a lock already registered for this path, or null if none exists. */
    public Semaphore getLock(Path filePath) {
        return fileLocks.get(normalize(filePath));
    }

    /** Acquires the exclusive lock for a file, waiting until it becomes available. */
    public void lock(Path filePath) throws InterruptedException {
        addLock(filePath).acquire();
    }

    /** Attempts to acquire the exclusive lock without waiting. */
    public boolean tryLock(Path filePath) {
        return addLock(filePath).tryAcquire();
    }

    /** Releases a lock previously acquired by this caller. */
    public void unlock(Path filePath) {
        Semaphore lock = getLock(filePath);
        if (lock == null) {
            throw new IllegalStateException("No lock is registered for path: " + normalize(filePath));
        }
        lock.release();
    }

    /** Returns whether a lock for this file is currently held. */
    public boolean isLocked(Path filePath) {
        Semaphore lock = getLock(filePath);
        return lock != null && lock.availablePermits() == 0;
    }

    /**
     * Finds locked regular files at or below a path. If the supplied path is a
     * regular file, it is checked directly; if it is a directory, it is walked
     * recursively. Only files are lockable, so directory locks are not checked.
     * CODEX: This lets move/delete handlers check a whole subtree before acting.
     */
    public List<Path> findLockedFiles(Path path) throws IOException {
        Path absolutePath = normalize(path);
        List<Path> lockedFiles = new ArrayList<>();

        if (Files.isRegularFile(absolutePath)) {
            if (isLocked(absolutePath)) {
                lockedFiles.add(absolutePath);
            }
            return lockedFiles;
        }

        if (!Files.isDirectory(absolutePath)) {
            return lockedFiles;
        }

        try (Stream<Path> paths = Files.walk(absolutePath)) {
            paths.filter(Files::isRegularFile)
                    .filter(this::isLocked)
                    .forEach(lockedFiles::add);
        }
        return lockedFiles;
    }

    /** Convenience check for commands that only need to know whether any file is locked. */
    public boolean hasLockedFiles(Path path) throws IOException {
        return !findLockedFiles(path).isEmpty();
    }

    private static Path normalize(Path path) {
        if (path == null) {
            throw new IllegalArgumentException("Path cannot be null");
        }
        // CODEX: Absolute normalized keys make relative aliases resolve to the same registry entry.
        return path.toAbsolutePath().normalize();
    }
}
