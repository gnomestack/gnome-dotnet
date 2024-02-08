


tpl_load:
    dotnet new install "{{ justfile_directory() }}/tpl/Gnome.Std.Template"

tpl_unload:
    dotnet new uninstall Gnome.Std.Template