using System.Security.Cryptography.X509Certificates;
using Microsoft.EntityFrameworkCore;
using sutnance.Data;
using sutnance.Models;

namespace sutnance.hacking;

public interface IAlertsManager
{
    List<Alert> GetAlerts();
    void Dismiss(int id);
    Task RaiseAlertIfNeeded(Machine machine,string? text = null);
}
public class AlertsManager(
    sutnanceContext _db
    ,IEmailSender _emailSender
    ) : IAlertsManager
{
    public async Task RaiseAlertIfNeeded(Machine machine, string? text = null)
    {
        // we will not raise an alert if we already have
        // an unresolved alert for that machine
        if (_db.alerts.Any(a => a.machineId == machine.Id && !a.resolved))
        {
            return;
        }
        var alert = new Alert(machine.Id);
        // insert an alert to the BD
        if (!string.IsNullOrEmpty(text)){
            alert.Details = text;
        }
        _db.alerts.Add(alert);
        await _db.SaveChangesAsync();
        // send an email
        string subject = $"la machine {machine.Name} a ete detruit";
        string body =
                $"ID: <b>{machine.Id}</b><br>" +
                $"Nom: <b>{machine.Name}</b><br>" +
                $"IP: <b>{machine.Ip}</b><br>" +
                $"Type: <b>{machine.Type}</b><br>" +
                $"date d'incident: <b>{DateTime.Now}</b><br>"
            ;
        _emailSender.SendAsync(machine.Manager,subject,body);
    }
    public void Dismiss(int id){
            var alert = _db.alerts.Find(id);
            if (alert == null) return;
            alert.resolved = true;
            _db.alerts.Update(alert);
            _db.SaveChanges();
    }

    public List<Alert> GetAlerts()
    {
        return _db.alerts.AsNoTracking().Where(a=>!a.resolved).ToList();
    }
}
