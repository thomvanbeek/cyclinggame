# -*- coding: utf-8 -*-
"""
Created on Thu Apr 12 21:38:01 2012

Bicycle simulator serious game 'hello world' toy example.
This file establishes a connection with a TCP server
It then sends an XML string to that server
The server should reply with a new XML string
This program prints the output in the std. output window

@author: Thom
"""
import time
from lxml import etree
from twisted.internet.protocol import ClientFactory
from twisted.protocols.basic import LineReceiver
from twisted.internet import reactor
from twisted.internet.task import LoopingCall

import optparse

def parse_args():
    usage = """usage: %prog [options] poetry-file
        This is the phantom xml reply server to mimic the game, Twisted edition.
        Run it like this: python simpleTwistedTcpServer.py --iface <ip adres to serve> --port <port>
        
        """

    parser = optparse.OptionParser(usage)

    help = "The port to listen on. Default to a random available port."
    parser.add_option('--port', type='int', help=help, default=13000)

    help = "The interface to listen on. Default is localhost."
    parser.add_option('--iface', help=help, default='localhost')

    options, args = parser.parse_args()

    return options

# Values to send to server
DATA = {'angle': 5, 'speed': 3, 'brake': 0}
#DATA = {'delta': 0, 'roll': -0, 'yaw': 0, 'v': 0, 'kill': 0}

class XmlEcho(LineReceiver):
    
    def lineReceived(self, line):
        try:
            print "Received reply..:"
            root = etree.XML(line)
            print etree.tostring(root, pretty_print = True)
            
        except:
            print "The client was not able to reconstruct an XML form the received string. The following was received from the server: "+line

    def connectionMade(self):
        print "eerst?"
        #pass
        self.sendMessage()
        self.lc = LoopingCall(self.sendMessage)
        self.lc.start(1)
            
    def sendMessage(self):
        try:
            self.sendLine(etree.tostring(self.factory.xml))
            print "Message send.. waiting for reply.."
        except:
            self.sendLine(self.factory.xml)
            print 'error of not being able to send line as xml as expected'
        

class XmlEchoClientFactory(ClientFactory):

    protocol = XmlEcho    
    
    def __init__(self, xml):
        self.xml = xml
    
    def startedConnecting(self, connector):
        print 'Started to connect.'

    def clientConnectionLost(self, connector, reason):
        print 'Lost connection.  Reason:', reason

    def clientConnectionFailed(self, connector, reason):
        print 'Connection failed. Reason:', reason
        
    def clientConnectionMade(self, connector, reason):
        print "iets"

def xmlClient_main():
    
    #Create the XML string for parsing later on
    root = etree.Element("root")
    for key, val in DATA.iteritems():
        el = etree.SubElement(root, key)
        el.text = str(val)
    
    print "The XML string send to the server is:\n"+etree.tostring(root, pretty_print=True)
    print "But the XML is send as a line:\n"+etree.tostring(root)+"\n"
    options = parse_args()
    
    reactor.connectTCP(options.iface, options.port, XmlEchoClientFactory(root))
    reactor.run()

if __name__ == '__main__':
    xmlClient_main()
