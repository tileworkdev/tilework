namespace Tilework.Core.Enums;

public enum UnixSignal
{
    // Hangup detected on controlling terminal or death of controlling process
    SIGHUP = 1, 
    
    // Interrupt from keyboard
    SIGINT = 2,  
    
    // Quit from keyboard
    SIGQUIT = 3, 
    
    // Illegal instruction
    SIGILL = 4,  
    
    // Trace/breakpoint trap
    SIGTRAP = 5,  
    
    // Abort signal from abort()
    SIGABRT = 6,  
    
    // Bus error (bad memory access)
    SIGBUS = 7,  
    
    // Floating-point exception
    SIGFPE = 8,  
    
    // Kill signal
    SIGKILL = 9,  
    
    // User-defined signal 1
    SIGUSR1 = 10, 
    
    // Invalid memory reference
    SIGSEGV = 11, 
    
    // User-defined signal 2
    SIGUSR2 = 12, 
    
    // Broken pipe: write to pipe with no readers
    SIGPIPE = 13, 
    
    // Timer signal from alarm()
    SIGALRM = 14, 
    
    // Termination signal
    SIGTERM = 15, 
    
    // Stack fault on coprocessor
    SIGSTKFLT = 16, 
    
    // Child stopped or terminated
    SIGCHLD = 17, 
    
    // Continue if stopped
    SIGCONT = 18, 
    
    // Stop process
    SIGSTOP = 19, 
    
    // Stop typed at terminal
    SIGTSTP = 20, 
    
    // Terminal input for background process
    SIGTTIN = 21, 
    
    // Terminal output for background process
    SIGTTOU = 22, 
    
    // Urgent condition on socket
    SIGURG = 23, 
    
    // CPU time limit exceeded
    SIGXCPU = 24, 
    
    // File size limit exceeded
    SIGXFSZ = 25, 
    
    // Virtual alarm clock
    SIGVTALRM = 26, 
    
    // Profiling timer expired
    SIGPROF = 27, 
    
    // Window resize signal
    SIGWINCH = 28, 
    
    // I/O now possible
    SIGIO = 29, 
    
    // Power failure restart
    SIGPWR = 30, 
    
    // Bad system call (unused)
    SIGSYS = 31, 
    
    // Real-time signals (example range)
    SIGRTMIN = 34, 
    SIGRTMAX = 64
}



public static class DockerSignalMapper
{
    public static string MapUnixSignalToDockerSignal(UnixSignal signal)
    {
        // Convert the UnixSignal to a Docker-compatible string
        return signal switch
        {
            UnixSignal.SIGHUP => "HUP",
            UnixSignal.SIGINT => "INT",
            UnixSignal.SIGQUIT => "QUIT",
            UnixSignal.SIGILL => "ILL",
            UnixSignal.SIGTRAP => "TRAP",
            UnixSignal.SIGABRT => "ABRT",
            UnixSignal.SIGBUS => "BUS",
            UnixSignal.SIGFPE => "FPE",
            UnixSignal.SIGKILL => "KILL",
            UnixSignal.SIGUSR1 => "USR1",
            UnixSignal.SIGSEGV => "SEGV",
            UnixSignal.SIGUSR2 => "USR2",
            UnixSignal.SIGPIPE => "PIPE",
            UnixSignal.SIGALRM => "ALRM",
            UnixSignal.SIGTERM => "TERM",
            UnixSignal.SIGSTKFLT => "STKFLT",
            UnixSignal.SIGCHLD => "CHLD",
            UnixSignal.SIGCONT => "CONT",
            UnixSignal.SIGSTOP => "STOP",
            UnixSignal.SIGTSTP => "TSTP",
            UnixSignal.SIGTTIN => "TTIN",
            UnixSignal.SIGTTOU => "TTOU",
            UnixSignal.SIGURG => "URG",
            UnixSignal.SIGXCPU => "XCPU",
            UnixSignal.SIGXFSZ => "XFSZ",
            UnixSignal.SIGVTALRM => "VTALRM",
            UnixSignal.SIGPROF => "PROF",
            UnixSignal.SIGWINCH => "WINCH",
            UnixSignal.SIGIO => "IO",
            UnixSignal.SIGPWR => "PWR",
            UnixSignal.SIGSYS => "SYS",
            UnixSignal.SIGRTMIN => "RTMIN",
            UnixSignal.SIGRTMAX => "RTMAX",
            _ => throw new ArgumentOutOfRangeException(nameof(signal), $"Unsupported UnixSignal: {signal}")
        };
    }
}
