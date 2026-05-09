[global::System.Configuration.UserScopedSettingAttribute()]
[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
[global::System.Configuration.DefaultSettingValueAttribute("False")]
public bool RememberMe {
    get {
        return ((bool)(this["RememberMe"]));
    }
    set {
        this["RememberMe"] = value;
    }
}

[global::System.Configuration.UserScopedSettingAttribute()]
[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
[global::System.Configuration.DefaultSettingValueAttribute("")]
public string LoginName {
    get {
        return ((string)(this["LoginName"]));
    }
    set {
        this["LoginName"] = value;
    }
}

[global::System.Configuration.UserScopedSettingAttribute()]
[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
[global::System.Configuration.DefaultSettingValueAttribute("")]
public string Password {
    get {
        return ((string)(this["Password"]));
    }
    set {
        this["Password"] = value;
    }
}