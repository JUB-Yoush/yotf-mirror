using Godot;
using System;
using Yotf;

public partial class PauseMenu : Control
{
    private AudioManager Audio = null!;
    private Panel main = null!, settings = null!;
    private Button resume = null!, settingsBtn = null!, quit = null!, back = null!, testBtn = null!;

    public override void _Ready()
    {
        GetTree().Paused = true;
        Input.MouseMode = Input.MouseModeEnum.Visible;
        resume = GetNode<Button>("Paused/MainPaused/ResumeBtn");
        settingsBtn = GetNode<Button>("Paused/MainPaused/SettingsBtn");
        quit = GetNode<Button>("Paused/MainPaused/QuitBtn");
        back = GetNode<Button>("Paused/Settings/BackBtn");
        testBtn = GetNode<Button>("Paused/Settings/TestBtn");
        resume.Pressed += Resume;
        settingsBtn.Pressed += Settings;
        quit.Pressed += Quit;
        back.Pressed += Back;
        testBtn.Pressed += Test;
        main = GetNode<Panel>("Paused/MainPaused");
        settings = GetNode<Panel>("Paused/Settings");
    }

    public void Resume()
    {
        ButtonPressed(SFX.MenuClose);
        Input.MouseMode = Input.MouseModeEnum.Captured;
        main.Visible = true;
        settings.Visible = false;
        GetTree().Paused = false;
        QueueFree();
    }

    void Quit()
    {
        ButtonPressed(SFX.MenuClose);
        GetTree().Paused = false;
        GetTree().Quit();
        GetViewport().SetInputAsHandled();
    }

    void Settings()
    {
        main.Visible = false;
        settings.Visible = true;
        ButtonPressed(SFX.UIIncrease);
    }

    void Back()
    {
        main.Visible = true;
        settings.Visible = false;
        ButtonPressed(SFX.UIDecrease);
    }

    void Test()
    {
        ButtonPressed(SFX.UISelect);
    }

    void ButtonPressed(AudioStream btnSound)
    {
        AudioManager.PlaySfx(btnSound);
    }
}
