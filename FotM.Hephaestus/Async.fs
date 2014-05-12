namespace FotM.Hephaestus

module Async =
    type Agent<'T> = MailboxProcessor<'T>
